using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using System.IO.Pipelines;
using System.Text;
using System.Text.RegularExpressions;
using Lemur.Debug;

namespace Tests.Debug;

public class FakeHostApplicationLigetime : IHostApplicationLifetime
{
    public CancellationToken ApplicationStarted => CancellationToken.None;

    public CancellationToken ApplicationStopped => CancellationToken.None;

    public CancellationToken ApplicationStopping => CancellationToken.None;

    public void StopApplication()
    {
    }
}

public sealed partial class GdbConnectionFixture : ConnectionContext, IAsyncDisposable
{
    private readonly Pipe m_InPipe = new();   // Handler reads from here
    private readonly Pipe m_OutPipe = new();  // Handler writes to here
    private readonly MockEmulator m_Emulator;
    private readonly Task m_HandlerTask;

    public GdbConnectionFixture()
    {
        Transport = new DuplexPipe(m_InPipe.Reader, m_OutPipe.Writer);
        ConnectionId = Guid.NewGuid().ToString();
        Items = new Dictionary<object, object?>();
        Features = new FeatureCollection();

        m_Emulator = new MockEmulator();
        var handler = new GdbConnectionHandler(new NullLogger<GdbConnectionHandler>(), m_Emulator, new FakeHostApplicationLigetime());
        m_HandlerTask = handler.OnConnectedAsync(this);
    }

    public override string ConnectionId { get; set; }
    public override IDuplexPipe Transport { get; set; }
    public override IFeatureCollection Features { get; }
    public override IDictionary<object, object?> Items { get; set; }
    public override void Abort(ConnectionAbortedException abortReason) { }

    public MockEmulator Emulator => m_Emulator;

    public async Task<string> SendPacketAsync(string packet, CancellationToken cancellationToken = default)
    {
        await m_InPipe.Writer.WriteAsync(Encoding.UTF8.GetBytes($"${packet}#{CalculateChecksum(packet)}"), cancellationToken);
        await m_InPipe.Writer.CompleteAsync();

        var stringBuilder = new StringBuilder();
        while (true)
        {
            var result = await m_OutPipe.Reader.ReadAsync(cancellationToken);

            foreach (var segment in result.Buffer)
            {
                stringBuilder.Append(Encoding.UTF8.GetString(segment.Span));
            }

            m_OutPipe.Reader.AdvanceTo(result.Buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await m_OutPipe.Reader.CompleteAsync();
        var reply = stringBuilder.ToString();

        var match = ReplyRegex().Match(reply);
        Assert.True(match.Success, "Reply message should match pattern");
        Assert.Equal(CalculateChecksum(match.Groups[1].Value), match.Groups[2].Value, ignoreCase: true);
        return match.Groups[1].Value;
    }

    [GeneratedRegex(@"^\+?\$(.*)\#([\dA-Fa-f]{2})$")]
    private partial Regex ReplyRegex();

    private string CalculateChecksum(string msg)
    {
        byte checksum = 0;
        foreach (char c in msg)
        {
            checksum += (byte)c;
        }

        return $"{checksum:x2}";
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await m_InPipe.Writer.CompleteAsync();
        await m_HandlerTask;
    }

    private sealed class DuplexPipe(PipeReader input, PipeWriter output) : IDuplexPipe
    {
        public PipeReader Input { get; } = input;
        public PipeWriter Output { get; } = output;
    }
}
