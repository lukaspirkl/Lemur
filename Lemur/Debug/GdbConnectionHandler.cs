using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Lemur.Debug;

// https://sourceware.org/gdb/current/onlinedocs/gdb.html/Remote-Protocol.html
// https://medium.com/swlh/implement-gdb-remote-debug-protocol-stub-from-scratch-1-a6ab2015bfc5

public partial class GdbConnectionHandler : ConnectionHandler
{
    private readonly ILogger<GdbConnectionHandler> m_Logger;
    private readonly IDebuggable m_Emulator;
    private readonly IHostApplicationLifetime m_HostLifetime;
    private readonly GdbCommandRouter m_Router;

    [GeneratedRegex(@"^G[0-9A-Fa-f]+$")]
    private static partial Regex WriteAllRegistersPattern();

    [GeneratedRegex(@"^P[0-9A-Fa-f]+=[0-9A-Fa-f]+$")]
    private static partial Regex WriteRegisterPattern();

    [GeneratedRegex(@"^m[0-9A-Fa-f]+,[0-9A-Fa-f]+$")]
    private static partial Regex ReadMemoryPattern();

    [GeneratedRegex(@"^M[0-9A-Fa-f]+,[0-9A-Fa-f]+:[0-9A-Fa-f]+$")]
    private static partial Regex WriteMemoryPattern();

    public GdbConnectionHandler(ILogger<GdbConnectionHandler> logger, IDebuggable emulator, IHostApplicationLifetime hostLifetime)
    {
        m_Logger = logger;
        m_Emulator = emulator;
        m_HostLifetime = hostLifetime;
        m_Router = BuildRouter();
    }

    private GdbCommandRouter BuildRouter()
    {
        var router = new GdbCommandRouter();

        router.Map("?",               _ => "S05");
        router.MapPrefix("qSupported",_ => "PacketSize=400;hwbreak+;hwbreak+;vContSupported+;multiprocess-");
        router.Map("vMustReplyEmpty", _ => "");
        router.Map("qfThreadInfo",    _ => "m1");
        router.Map("qsThreadInfo",    _ => "1");
        router.Map("qC",              _ => "1");
        router.Map("qSymbol::",       _ => "OK");
        router.MapPrefix("Hg",        _ => "OK");
        router.MapPrefix("Hc",        _ => "OK");
        router.Map("qOffsets",        _ => "Text=0;Data=0;Bss=0");
        router.Map("qTStatus",        _ => "");
        router.Map("qAttached",       _ => "1");
        router.Map("vCont?",          _ => "vCont;c;C;s;S");
        router.MapPrefix("vCont;s",   _ => { m_Emulator.Step(); return "T05"; });
        router.MapPrefix("vCont;c",   _ => { m_Emulator.Run(); return null; });
        router.MapPrefix("vCont;t",   _ => { m_Emulator.Stop(); return "S05"; });
        router.MapPrefix("qRcmd,",    HandleQRcmd);
        router.Map("g",               HandleReadAllRegisters);
        router.MapPattern(WriteAllRegistersPattern(), HandleWriteAllRegisters);
        router.MapPattern(WriteRegisterPattern(),     HandleWriteRegister);
        router.MapPattern(ReadMemoryPattern(),        HandleMemoryRead);
        router.MapPattern(WriteMemoryPattern(),       HandleMemoryWrite);
        router.MapPrefix("Z",         HandleAddBreakpoint);
        router.MapPrefix("z",         HandleRemoveBreakpoint);
        router.MapPrefix("qXfer:",    HandleQXfer);

        return router;
    }

    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(connection.ConnectionClosed, m_HostLifetime.ApplicationStopping);
        var cancellationToken = cts.Token;

        m_Logger.LogInformation($"Debugger connected: {connection.RemoteEndPoint}");

        var input = connection.Transport.Input;
        var output = connection.Transport.Output;

        Action sendStopped = () =>
        {
            SendPacketAsync(output, "S05").Wait();
        };

        try
        {
            m_Emulator.Stopped += sendStopped;

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await input.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                if (buffer.Length == 1 && buffer.FirstSpan[0] == 0x3)
                {
                    m_Emulator.Stop();
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


                        string packet = Encoding.ASCII.GetString(packetData!);

                        m_Logger.LogDebug("GDB>> {packet}", packet);

                        var response = ProcessGdbCommand(packet);
                        if (response != null)
                        {
                            m_Logger.LogDebug("GDB<< {response}", response);

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
            m_Logger.LogInformation("GDB connection cancelled");
        }
        catch (ConnectionResetException e)
        {
            m_Logger.LogWarning(e, "Connection reset");
        }
        catch (Exception e)
        {
            m_Logger.LogError(e, "Unexpected exception while handling GDB connection");
        }
        finally
        {
            m_Emulator.Stopped -= sendStopped;
            m_Logger.LogInformation("Debugger disconnected.");
        }
    }

    private string? ProcessGdbCommand(string command)
    {
        using var _ = m_Logger.BeginScope("GDB command: {gdbCommand}", command);

        if (!m_Router.TryDispatch(command, out var response))
        {
            m_Logger.LogWarning("Unknown GDB command: {command}", command);
            return "";
        }

        return response;
    }

    private string? HandleQRcmd(string command)
    {
        var str = Encoding.ASCII.GetString(HexStringToBytes(command.Substring(6)));

        if (str is "reset halt" or "reset init")
        {
            m_Emulator.Reset();
            return "OK";
        }

        m_Logger.LogWarning("Unknown qRcmd: {cmd}", str);
        return "";
    }

    private string HandleReadAllRegisters(string _)
    {
        try
        {
            int registerCount = (int)m_Emulator.Registers.Length;
            int hexCharsPerReg = 8;
            int totalLength = registerCount * hexCharsPerReg;

            Span<char> response = stackalloc char[totalLength];
            int position = 0;

            for (int i = 0; i < registerCount; i++)
            {
                uint regValue = m_Emulator.Registers[(uint)i];
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

    private string HandleWriteAllRegisters(string command)
    {
        var hexData = command.Substring(1);
        if (hexData.Length != m_Emulator.Registers.Length * 8)
            return "E01";

        try
        {
            for (int i = 0; i < m_Emulator.Registers.Length; i++)
                m_Emulator.Registers[(uint)i] = ParseLittleEndianHex(hexData.AsSpan(i * 8, 8));

            return "OK";
        }
        catch (Exception e)
        {
            m_Logger.LogError(e, "Unexpected exception when getting register data");
            return "E02";
        }
    }

    private string HandleWriteRegister(string command)
    {
        var args = command.Substring(1).Split('=');
        if (args.Length != 2)
            return "E01";

        var reg = Convert.ToUInt32(args[0], 16);
        var value = BitConverter.ToUInt32(HexStringToBytes(args[1]));
        m_Emulator.Registers[reg] = value;
        return "OK";
    }

    private string HandleMemoryRead(string command)
    {
        try
        {
            var args = command.Substring(1).Split(',');
            var address = Convert.ToUInt32(args[0], 16);
            var length = Convert.ToUInt32(args[1], 16);

            return string.Join("", m_Emulator.MemoryRead(address, (int)length).Select(x => Convert.ToString(x, 16).PadLeft(2, '0')));
        }
        catch (Exception e)
        {
            m_Logger.LogError(e, "Unexpected exception while reading memory");
            return "E01";
        }
    }

    private string HandleMemoryWrite(string command)
    {
        try
        {
            var args = command.Substring(1).Split(',');
            var address = Convert.ToUInt32(args[0], 16);
            var args2 = args[1].Split(':');
            var hexData = args2[1];

            var data = new byte[hexData.Length / 2];
            for (int i = 0; i < data.Length; i++)
                data[i] = Convert.ToByte(hexData.Substring(i * 2, 2), 16);

            m_Emulator.MemoryWrite(address, data);
            return "OK";
        }
        catch (Exception e)
        {
            m_Logger.LogError(e, "Unexpected exception while writing memory");
            return "E01";
        }
    }

    private string HandleAddBreakpoint(string command)
    {
        var args = command.Split(',');
        var address = Convert.ToUInt32(args[1], 16);
        m_Emulator.Brakpoints.Add(address);
        return "OK";
    }

    private string HandleRemoveBreakpoint(string command)
    {
        var args = command.Split(',');
        var address = Convert.ToUInt32(args[1], 16);
        m_Emulator.Brakpoints.Remove(address);
        return "OK";
    }

    private string HandleQXfer(string command)
    {
        var c = command.Split(':');
        // qXfer:features:read:target.xml:OFFSET,LENGTH
        if (c[1] == "features" && c[2] == "read" && c[3] == "target.xml")
        {
            var args = c[4].Split(",").Select(hex => Convert.ToInt32(hex, 16)).ToArray();
            var offset = args[0];
            var length = args[1];

            if (offset > m_Targetxml.Length)
                return "l";

            int remaining = m_Targetxml.Length - offset;
            int chunkSize = Math.Min(length, remaining);
            string chunk = m_Targetxml.Substring(offset, chunkSize);

            char indicator = (chunkSize == remaining) ? 'l' : 'm';
            return indicator + chunk;
        }

        return "";
    }

    // Helper to find '$...#CC' in the buffer
    private bool TryFindPacket(ReadOnlySequence<byte> buffer, out byte[]? packetData, out SequencePosition consumedTo)
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

    private string m_Targetxml = """
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
