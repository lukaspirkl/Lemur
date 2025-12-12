using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Text;
using System.Text.RegularExpressions;

namespace Venture.Debug;

// https://sourceware.org/gdb/current/onlinedocs/gdb.html/Remote-Protocol.html
// https://medium.com/swlh/implement-gdb-remote-debug-protocol-stub-from-scratch-1-a6ab2015bfc5


public class GdbConnectionHandler : ConnectionHandler
{
    private readonly ILogger<GdbConnectionHandler> _logger;
    private readonly IDebuggable emulator;
    private readonly IHostApplicationLifetime hostLifetime;

    public GdbConnectionHandler(ILogger<GdbConnectionHandler> logger, IDebuggable emulator, IHostApplicationLifetime hostLifetime)
    {
        _logger = logger;
        this.emulator = emulator;
        this.hostLifetime = hostLifetime;
    }

    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(connection.ConnectionClosed, hostLifetime.ApplicationStopping);
        var cancellationToken = cts.Token;

        _logger.LogInformation($"Debugger connected: {connection.RemoteEndPoint}");

        try
        {
            var input = connection.Transport.Input;
            var output = connection.Transport.Output;

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await input.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                if (buffer.Length == 1 && buffer.FirstSpan[0] == 0x3)
                {
                    emulator.Stop();
                    await SendPacketAsync(output, "S05");
                }

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

                        _logger.LogDebug("GDB>> {packet}", packet);

                        var response = ProcessGdbCommand(packet);
                        if (response != null)
                        {
                            _logger.LogDebug("GDB<< {response}", response);

                            await SendPacketAsync(output, response);
                        }

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

            output.Complete();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GDB connection cancelled");
        }
        catch (ConnectionResetException e)
        {
            _logger.LogWarning(e, "Connection reset");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception while handling GDB connection");
        }
        finally
        {
            _logger.LogInformation("Debugger disconnected.");
        }
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

    private string? ProcessGdbCommand(string command)
    {
        using var _ = _logger.BeginScope("GDB command: {gdbCommand}", command);

        if (command == "?")
        {
            return "S05"; // Use stop reason SIGTRAP(5).
        }


        if (command.StartsWith("qSupported"))
        {
            return "PacketSize=400;vContSupported+;multiprocess-";// ;qXfer:features:read+";
        }

        if (command == "vMustReplyEmpty")
        {
            // The correct reply to an unknown ‘v’ packet is to return the empty string.
            // The ‘vMustReplyEmpty’ is used as a feature test to check how gdbserver handles unknown packets, it is important
            // that this packet be handled in the same way as other unknown ‘v’ packets. If this packet is handled differently
            // to other unknown ‘v’ packets then it is possible that GDB may run into problems in other areas,
            // specifically around use of ‘vFile:setfs:’.
            return "";
        }

        if (command == "qfThreadInfo")
        {
            return "m1";
        }

        if (command == "qsThreadInfo")
        {
            return "1";
        }

        if (command == "qC")
        {
            // Get current thread
            return "1";
        }

        if (command == "qSymbol::")
        {
            // Notify the target that GDB is prepared to serve symbol lookup requests. Accept requests from the target for the values of symbols.
            return "OK"; // The target does not need to look up any (more) symbols
        }

        if (command.StartsWith("Hg"))
        {
            // Set thread for subsequent operations
            // Hc-1  - all threads
            // Hc1   - thread 1
            return "OK";
        }

        if (command.StartsWith("Hc"))
        {
            // Selector for the thread used for continue operations.
            // Hc-1  - select all threads for continue
            // Hc1   - select thread 1 for continue
            return "OK";
        }

        if (command == "qOffsets")
        {
            // This queries relocation offsets between sections, normally used by targets with dynamic loading or strange memory maps.
            return "Text=0;Data=0;Bss=0";
        }

        if (command == "qTStatus")
        {
            // https://sourceware.org/gdb/current/onlinedocs/gdb.html/Tracepoint-Packets.html#Tracepoint-Packets
            // Return nothing as I am not supporting tracepoints
            return "";
        }

        if (command == "qAttached")
        {
            //Return an indication of whether the remote server attached to an existing process or created a new process.
            // 1 - attached to existing process
            // 0 - created new process
            return "1";
        }

        if (command == "vCont?")
        {
            // Supported vCont actions.
            // s - step
            // c - continue
            return "vCont;c;C;s;S";
        }

        if (command.StartsWith("vCont;s"))
        {
            emulator.Step();
            return "T05";
        }

        if (command.StartsWith("vCont;c"))
        {
            emulator.Run();
            return null;
        }

        if (command.StartsWith("vCont;t"))
        {
            emulator.Stop();
            return "S05";
        }

        if (command.StartsWith("qRcmd,"))
        {
            var str = Encoding.ASCII.GetString(HexStringToBytes(command.Substring(6)));
            if (str == "reset halt")
            {
                // TODO: Reset
            }
        }

        if (command == "g")
        {
            try
            {
                int registerCount = emulator.Registers.Length;
                int hexCharsPerReg = 8;
                int totalLength = registerCount * hexCharsPerReg;

                Span<char> response = stackalloc char[totalLength];
                int position = 0;

                for (int i = 0; i < registerCount; i++)
                {
                    uint regValue = emulator.Registers[(uint)i];
                    WriteLittleEndianHex(regValue, response.Slice(position, hexCharsPerReg));
                    position += hexCharsPerReg;
                }

                return new string(response);
            }
            catch (Exception)
            {
                return "E01";
            }
        }

        // Read registers
        if (Regex.IsMatch(command, @"G[0-9ABCDEFabcdef]+"))
        {
            var hexData = command.Substring(1);
            if (hexData.Length != emulator.Registers.Length * 8)
            {
                return "E01"; // Error: insufficient data
            }

            try
            {
                for (int i = 0; i < emulator.Registers.Length; i++)
                {
                    emulator.Registers[(uint)i] = ParseLittleEndianHex(hexData.AsSpan(i * 8, 8));
                }

                return "OK";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected exception when getting register data");
                return "E02"; // Error: parsing failed
            }
        }

        if (Regex.IsMatch(command, @"P[0-9ABCDEFabcdef]+=[0-9ABCDEFabcdef]"))
        {
            var args = command.Substring(1).Split('=');
            if (args.Length != 2)
            {
                return "E01";
            }

            var reg = Convert.ToUInt32(args[0], 16);
            var value = BitConverter.ToUInt32(HexStringToBytes(args[1]));

            emulator.Registers[reg] = value;
            return "OK";
        }

        // Read memory - mADDR,LEN
        if (Regex.IsMatch(command, @"m[0-9abcdefABCDEF]+,[0-9abcdefABCDEF]+"))
        {
            try
            {
                var args = command.Substring(1).Split(',');
                var address = Convert.ToUInt32(args[0], 16);
                var length = Convert.ToUInt32(args[1], 16);

                // TODO: Not sure if the endianness is correct
                return string.Join("", emulator.MemoryRead(address, (int)length).Select(x => Convert.ToString(x, 16).PadLeft(2, '0')));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected exception while reading memory");
                return "E01";
            }
        }

        // Write memory - MADDR,LEN:DATA
        if (Regex.IsMatch(command, @"M[0-9abcdefABCDEF]+,[0-9abcdefABCDEF]+:[0-9abcdefABCDEF]+"))
        {
            try
            {
                var args = command.Substring(1).Split(',');
                var address = Convert.ToUInt32(args[0], 16);
                var args2 = args[1].Split(':');
                var length = Convert.ToUInt32(args2[0], 16);
                var hexData = args2[1];

                var data = new byte[hexData.Length / 2];
                for (int i = 0; i < data.Length; i++)
                {
                    // TODO: Not sure if the endianness is correct
                    data[i] = Convert.ToByte(hexData.Substring(i * 2, 2), 16);
                }
                emulator.MemoryWrite(address, data);

                return "OK";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected exception while writing memory");
                return "E01";
            }
        }

        if (command.StartsWith("qXfer:"))
        {
            var c = command.Split(':');
            // qXfer:features:read:target.xml:OFFSET,LENGTH
            if (c[1] == "features" && c[2] == "read" && c[3] == "target.xml")
            {
                var args = c[4].Split(",").Select(hex => Convert.ToInt32(hex, 16)).ToArray();
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
        }

        _logger.LogWarning("Unknown GDB command: {command}", command);
        return ""; // Empty = Not Supported (Correct for vMustReplyEmpty)
    }

    private uint ParseBigEndianHex(ReadOnlySpan<char> hex)
    {
        // TODO: Isn't this too complicated?
        Span<byte> bytes = stackalloc byte[(hex.Length + (hex.Length % 2)) / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[bytes.Length - 1 - i] = Convert.ToByte(hex.Slice(i * 2, 2).ToString(), 16);
        }
        return BitConverter.ToUInt32(bytes);
    }

    private uint ParseLittleEndianHex(ReadOnlySpan<char> hex)
    {
        // TODO: Isn't this too complicated?
        Span<byte> bytes = stackalloc byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Slice(i * 2, 2).ToString(), 16);
        }
        return BitConverter.ToUInt32(bytes);
    }

    private void WriteLittleEndianHex(uint value, Span<char> destination)
    {
        // Write bytes in little-endian order (LSB first)
        for (int i = 0; i < 4; i++)
        {
            byte b = (byte)(value >> (i * 8));
            destination[i * 2] = GetHexChar(b >> 4);
            destination[i * 2 + 1] = GetHexChar(b & 0x0F);
        }
    }

    private static byte[] HexStringToBytes(string hex)
    {
        if (hex.Length % 2 != 0) throw new ArgumentException("Hex string must have even length");
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    private char GetHexChar(int value)
    {
        return value < 10 ? (char)('0' + value) : (char)('a' + value - 10);
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
