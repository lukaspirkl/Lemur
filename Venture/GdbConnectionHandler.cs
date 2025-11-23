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
                    // Send ACK
                    await output.WriteAsync(new byte[] { (byte)'+' });


                    string packet = Encoding.ASCII.GetString(packetData);

                    Console.WriteLine($">> {packet}");

                    string response = ProcessGdbCommand(packet.Split(':'));

                    Console.WriteLine($"<< {response}");

                    await SendPacketAsync(output, response);

                    // Update the buffer to skip what we just processed
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

    private string ProcessGdbCommand(string[] command)
    {
        switch (command[0])
        {
            case "qSupported":
                return "PacketSize=400;qXfer:features:read+";

            //case "vCont": 
            //    return "vCont;c;s;t"; // "I support continue, step, and stop"

            //case "Hg": 
            //    return "OK";

            //case "Hc": 
            //    return "OK";

            case "?":
                return "S05"; // Use stop reason SIGTRAP(5).

            case "g":
                // Mock Registers
                var sb = new StringBuilder();
                for (int i = 0; i < 32; i++) sb.Append("00000000");
                sb.Append("FFFFFFFF");
                return sb.ToString();

            case "qXfer": 
                // qXfer:features:read:target.xml:OFFSET,LENGTH
                // 
                if (command[1] == "features" && command[2] == "read" && command[3] == "target.xml")
                {
                    var args = command[4].Split(",").Select(hex => Convert.ToInt32(hex, 16)).ToArray();
                    var offset = args[0];
                    var length = args[1];

                    if (offset > targetxml.Length)
                    {
                        return "l"; // nothing left
                    }

                    int remaining = targetxml.Length - offset;
                    int chunkSize = Math.Min(length, remaining);
                    string chunk = targetxml.Substring(offset, chunkSize);

                    char indicator = (chunkSize == remaining) ? 'l' : 'm';
                    return indicator + chunk;
                }
                return "";

            //case "m": 
            //    return "00000000";

            default:
                Console.WriteLine($"Unknown command: {command}");
                return ""; // Empty = Not Supported (Correct for vMustReplyEmpty)
        }
    }

    private async Task SendPacketAsync(System.IO.Pipelines.PipeWriter output, string payload)
    {
        byte checksum = 0;
        foreach (char c in payload)
        {
            checksum += (byte)c;
        }

        string packet = $"${payload}#{checksum:x2}";

        await output.WriteAsync(Encoding.ASCII.GetBytes(packet));
    }

    private string targetxml = """
        <?xml version="1.0"?>
        <!DOCTYPE target SYSTEM "gdb-target.dtd">
        <target>
          <architecture>riscv:rv32</architecture>

          <feature name="org.gnu.gdb.riscv.core">
            <reg name="x0"  bitsize="32"/>
            <reg name="x1"  bitsize="32"/>
            <reg name="x2"  bitsize="32"/>
            <reg name="x3"  bitsize="32"/>
            <reg name="x4"  bitsize="32"/>
            <reg name="x5"  bitsize="32"/>
            <reg name="x6"  bitsize="32"/>
            <reg name="x7"  bitsize="32"/>
            <reg name="x8"  bitsize="32"/>
            <reg name="x9"  bitsize="32"/>
            <reg name="x10" bitsize="32"/>
            <reg name="x11" bitsize="32"/>
            <reg name="x12" bitsize="32"/>
            <reg name="x13" bitsize="32"/>
            <reg name="x14" bitsize="32"/>
            <reg name="x15" bitsize="32"/>
            <reg name="x16" bitsize="32"/>
            <reg name="x17" bitsize="32"/>
            <reg name="x18" bitsize="32"/>
            <reg name="x19" bitsize="32"/>
            <reg name="x20" bitsize="32"/>
            <reg name="x21" bitsize="32"/>
            <reg name="x22" bitsize="32"/>
            <reg name="x23" bitsize="32"/>
            <reg name="x24" bitsize="32"/>
            <reg name="x25" bitsize="32"/>
            <reg name="x26" bitsize="32"/>
            <reg name="x27" bitsize="32"/>
            <reg name="x28" bitsize="32"/>
            <reg name="x29" bitsize="32"/>
            <reg name="x30" bitsize="32"/>
            <reg name="x31" bitsize="32"/>

            <reg name="pc"  bitsize="32"/>
          </feature>
        </target>
        
        """;
}
