using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Text;

namespace Venture;

// https://sourceware.org/gdb/current/onlinedocs/gdb.html/Remote-Protocol.html
// https://medium.com/swlh/implement-gdb-remote-debug-protocol-stub-from-scratch-1-a6ab2015bfc5


public class GdbConnectionHandler : ConnectionHandler
{
    private readonly ILogger<GdbConnectionHandler> _logger;

    public GdbConnectionHandler(ILogger<GdbConnectionHandler> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        _logger.LogInformation($"Debugger connected: {connection.RemoteEndPoint}");
        var input = connection.Transport.Input;
        var output = connection.Transport.Output;

        while (true)
        {
            var result = await input.ReadAsync();
            var buffer = result.Buffer;

            // We must track how much of the buffer we have used
            SequencePosition consumed = buffer.Start;
            SequencePosition examined = buffer.Start;

            try
            {
                // Loop through all available packets in the buffer
                while (TryFindPacket(buffer, out var packetData, out var consumedTo))
                {
                    // 1. Send ACK
                    await output.WriteAsync(new byte[] { (byte)'+' });

                    // 2. Process
                    string packet = Encoding.ASCII.GetString(packetData);
                    string command = packet.Split(':')[0];

                    // _logger.LogInformation($"Processing: {packet}"); 
                    string response = ProcessGdbCommand(command);

                    // 3. Respond
                    await SendPacketAsync(output, response);

                    // 4. Update the buffer to skip what we just processed
                    consumed = consumedTo;
                    buffer = buffer.Slice(consumed);

                    // Mark that we examined up to here
                    examined = consumed;
                }
            }
            finally
            {
                // Advance the pipe to where we stopped
                // If we didn't find a full packet, consumed stays at start (wait for more data)
                input.AdvanceTo(consumed, buffer.End);
            }

            if (result.IsCompleted) break;
        }
        _logger.LogInformation("Debugger disconnected.");
    }

    // Helper to find '$...#CC' in the buffer
    private bool TryFindPacket(ReadOnlySequence<byte> buffer, out byte[] packetData, out SequencePosition consumedTo)
    {
        packetData = null;
        consumedTo = buffer.Start;

        // Find Start '$'
        var start = buffer.PositionOf((byte)'$');
        if (start == null) return false;

        // Find End '#' (after the start)
        var dataAfterStart = buffer.Slice(start.Value);
        var hash = dataAfterStart.PositionOf((byte)'#');
        if (hash == null) return false;

        // Calculate position of checksum (2 bytes after #)
        // We need to verify the buffer actually has those 2 bytes
        var hashPos = dataAfterStart.GetPosition(0, hash.Value);

        // This is a simplified check. In production, you'd be more careful with boundaries.
        // We slice 1 byte past '#' to check length
        var afterHash = dataAfterStart.Slice(dataAfterStart.GetPosition(1, hash.Value));

        if (afterHash.Length < 2) return false; // Waiting for checksum bytes

        // We have a full packet! 
        // Extract content between $ and #
        // Start+1 to skip '$', length is up to '#'
        var contentBuffer = dataAfterStart.Slice(1, hash.Value);
        packetData = contentBuffer.ToArray();

        // The packet ends 2 bytes after the '#'
        consumedTo = afterHash.GetPosition(2);
        return true;
    }

    private string ProcessGdbCommand(string command)
    {
        switch (command)
        {
            case "qSupported": return "PacketSize=400";
            case "vCont": return "vCont;c;s;t"; // "I support continue, step, and stop"
            case "Hg": return "OK";
            case "Hc": return "OK";
            case "?": return "T05";
            case "g":
                // Mock Registers
                var sb = new StringBuilder();
                sb.Append("EFBEADDE"); // R0
                sb.Append("BEBAFECA"); // R1
                for (int i = 0; i < 14; i++) sb.Append("00000000");
                sb.Append("00000001");
                return sb.ToString();
            case "m": return "00000000";
            default: return ""; // Empty = Not Supported (Correct for vMustReplyEmpty)
        }
    }

    private async Task SendPacketAsync(System.IO.Pipelines.PipeWriter output, string payload)
    {
        byte checksum = 0;
        foreach (char c in payload) checksum += (byte)c;
        string packet = $"${payload}#{checksum:x2}";
        await output.WriteAsync(Encoding.ASCII.GetBytes(packet));
    }
}
