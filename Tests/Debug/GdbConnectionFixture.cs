using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using System.IO.Pipelines;
using System.Text;
using System.Text.RegularExpressions;
using Venture.Debug;

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
    private readonly Pipe _inPipe = new();   // Handler reads from here
    private readonly Pipe _outPipe = new();  // Handler writes to here
    private readonly MockEmulator _emulator;
    private readonly Task _handlerTask;

    public GdbConnectionFixture()
    {
        Transport = new DuplexPipe(_inPipe.Reader, _outPipe.Writer);
        ConnectionId = Guid.NewGuid().ToString();
        Items = new Dictionary<object, object?>();
        Features = new FeatureCollection();

        _emulator = new MockEmulator();
        var handler = new GdbConnectionHandler(new NullLogger<GdbConnectionHandler>(), _emulator, new FakeHostApplicationLigetime());
        _handlerTask = handler.OnConnectedAsync(this);
    }

    public override string ConnectionId { get; set; }
    public override IDuplexPipe Transport { get; set; }
    public override IFeatureCollection Features { get; }
    public override IDictionary<object, object?> Items { get; set; }
    public override void Abort(ConnectionAbortedException abortReason) { }

    public MockEmulator Emulator => _emulator;

    public async Task<string> SendPacketAsync(string packet, CancellationToken cancellationToken = default)
    {
        await _inPipe.Writer.WriteAsync(Encoding.UTF8.GetBytes($"${packet}#{CalculateChecksum(packet)}"), cancellationToken);
        await _inPipe.Writer.CompleteAsync();

        var stringBuilder = new StringBuilder();
        while (true)
        {
            var result = await _outPipe.Reader.ReadAsync(cancellationToken);

            foreach (var segment in result.Buffer)
            {
                stringBuilder.Append(Encoding.UTF8.GetString(segment.Span));
            }

            _outPipe.Reader.AdvanceTo(result.Buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await _outPipe.Reader.CompleteAsync();
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
        await _inPipe.Writer.CompleteAsync();
        await _handlerTask;
    }

    private sealed class DuplexPipe(PipeReader input, PipeWriter output) : IDuplexPipe
    {
        public PipeReader Input { get; } = input;
        public PipeWriter Output { get; } = output;
    }
}
