using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using System.IO.Pipelines;
using System.Text;
using Venture;

namespace Tests;


public sealed class TestConnectionContext : ConnectionContext
{
    private readonly IDuplexPipe _transport;
    private readonly FeatureCollection _features = new();

    public TestConnectionContext(PipeReader input, PipeWriter output)
    {
        _transport = new DuplexPipe(input, output);
        ConnectionId = Guid.NewGuid().ToString();
        Items = new Dictionary<object, object?>();
    }

    public override string ConnectionId { get; set; }

    public override IDuplexPipe Transport
    {
        get => _transport;
        set => throw new NotSupportedException();
    }

    public override IFeatureCollection Features => _features;

    public override IDictionary<object, object?> Items { get; set; }

    public override void Abort(ConnectionAbortedException abortReason)
    {
        // No-op for tests
    }

    private sealed class DuplexPipe : IDuplexPipe
    {
        public DuplexPipe(PipeReader input, PipeWriter output)
        {
            Input = input;
            Output = output;
        }

        public PipeReader Input { get; }
        public PipeWriter Output { get; }
    }
}


public class GdbConnectionHandlerTests
{
    [Fact]
    public async Task Test()
    {
        //var inPipe = new Pipe();
        //var outPipe = new Pipe();

        //var context = new TestConnectionContext(inPipe.Reader, outPipe.Writer);


        //var handler = new GdbConnectionHandler();
        //var handlerTask = handler.OnConnectedAsync(context);

        //await inPipe.Writer.WriteAsync(Encoding.UTF8.GetBytes("ping"), TestContext.Current.CancellationToken);
        //inPipe.Writer.Complete();

        ////var result = await ReadAll(outPipe.Reader);


        ////Assert.AreEqual("ping", Encoding.UTF8.GetString(result));

        //await handlerTask;
    }
}
