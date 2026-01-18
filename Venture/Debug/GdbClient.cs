using System.Net.Sockets;
using System.Text;
using Venture.Processor;

namespace Venture.Debug;

public class RP2350GDB : IDebuggable
{
    private readonly GdbClient m_GdbClient;

    private static SemaphoreSlim m_Semaphore = new SemaphoreSlim(1);

    public event Action? Stopped;

    public event Action? EBreak;

    public RP2350GDB(string host, int port)
    {
        m_Semaphore.Wait();
        m_GdbClient = new GdbClient(host, port);
        Registers = new GdbRegisters(m_GdbClient);
    }

    public IRegisters Registers { get; }

    public HashSet<uint> Brakpoints { get; } = new HashSet<uint>();

    public void Dispose()
    {
        m_GdbClient.Dispose();
        m_Semaphore.Release();
    }

    public byte[] MemoryRead(uint address, int count)
    {
        return m_GdbClient.ReadMemory(address, (uint)count);
    }

    public void MemoryWrite(uint address, byte[] data)
    {
        m_GdbClient.WriteMemory(address, data);
    }

    public void Run()
    {
        throw new NotImplementedException();
    }

    public void Step()
    {
        m_GdbClient.Step();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        m_GdbClient.Monitor("reset halt");
    }

    public uint GetCSR(ushort index)
    {
        // 32 base registers, 32 float registers and PC register = 65
        return m_GdbClient.ReadRegister((uint)(index + 65));
    }

    public void SetCSR(ushort index, uint value)
    {
        // 32 base registers, 32 float registers and PC register = 65
        m_GdbClient.WriteRegister((uint)(index + 65), value);
    }

    class GdbRegisters : IRegisters
    {
        private readonly GdbClient m_GdbClient;

        public GdbRegisters(GdbClient gdbClient)
        {
            m_GdbClient = gdbClient;
        }

        public uint this[uint index]
        {
            get
            {
                return m_GdbClient.ReadRegister(index);
            }
            set
            {
                m_GdbClient.WriteRegister(index, value);
            }
        }

        public uint Length => 33;
    }
}

public class GdbClient : IDisposable
{
    private TcpClient m_Client;
    private NetworkStream m_Stream;
    private StreamReader m_Reader;
    private StreamWriter m_Writer;
    private readonly object m_Lock = new object();

    public bool IsConnected => m_Client != null && m_Client.Connected;

    public GdbClient(string host, int port)
    {
        Console.WriteLine($"[GDB] Connecting to {host}:{port}...");
        m_Client = new TcpClient();
        m_Client.Connect(host, port);
        m_Stream = m_Client.GetStream();

        // Using basic ASCII/binary reading
        m_Reader = new StreamReader(m_Stream, Encoding.ASCII);
        m_Writer = new StreamWriter(m_Stream, Encoding.ASCII) { AutoFlush = true };

        // Turn off Nagle's algorithm for lower latency
        m_Client.NoDelay = true;

        // Initial handshake usually involves disabling AckMode in modern GDB, 
        // but for a basic client, we will assume standard AckMode is active.
        // We verify connection by sending a halt reason query.
        string response = SendCommand("?");
        Console.WriteLine($"[GDB] Connected. Initial State: {response}");
    }

    public void Dispose()
    {
        m_Stream.Close();
        m_Stream.Dispose();
        m_Client.Close();
        m_Client.Dispose();
    }

    public string SendCommand(string commandData)
    {
        var response = "";
        SendCommand(commandData, r =>
        {
            response = r;
            return true;
        });
        return response;
    }

    public void SendCommand(string commandData, Func<string, bool> handleResponse)
    {
        lock (m_Lock)
        {
            string packet = FormatPacket(commandData);

            while (true)
            {
                WriteRaw(packet);

                char ack = ReadChar();
                if (ack == '+')
                {
                    break; // Packet accepted
                }
                else if (ack == '-')
                {
                    Console.WriteLine($"[GDB] Warning: Remote sent NACK for '{commandData}'. Retrying...");
                    continue; // Retry
                }
                else
                {
                    throw new Exception($"[GDB] Protocol Error: Expected ACK (+), got '{ack}'");
                }
            }

            while (!handleResponse(ReadResponsePacket()))
            { }
        }
    }

    private string FormatPacket(string data)
    {
        byte checksum = CalculateChecksum(data);
        return $"${data}#{checksum:x2}";
    }

    private byte CalculateChecksum(string data)
    {
        int sum = 0;
        foreach (char c in data)
        {
            sum += (byte)c;
        }
        return (byte)(sum % 256);
    }

    private void WriteRaw(string data)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(data);
        m_Stream.Write(bytes, 0, bytes.Length);
    }

    private char ReadChar()
    {
        int b = m_Stream.ReadByte();
        if (b == -1) throw new EndOfStreamException("GDB connection closed.");
        return (char)b;
    }

    private string ReadResponsePacket()
    {
        while (true)
        {
            char c = ReadChar();

            // Ignore logic to skip non-packet data (like debug prints) until we hit '$'
            if (c != '$') continue;

            StringBuilder payload = new StringBuilder();

            // Read until '#'
            while (true)
            {
                c = ReadChar();
                if (c == '#') break;
                payload.Append(c);
            }

            // Read Checksum (2 chars)
            char cs1 = ReadChar();
            char cs2 = ReadChar();
            string receivedChecksum = $"{cs1}{cs2}";

            // Verify Checksum
            byte calculated = CalculateChecksum(payload.ToString());
            byte received = Convert.ToByte(receivedChecksum, 16);

            if (calculated == received)
            {
                // Send ACK
                WriteRaw("+");
                return payload.ToString();
            }
            else
            {
                // Send NACK
                WriteRaw("-");
                Console.WriteLine("[GDB] Checksum mismatch. Requesting retransmit.");
            }
        }
    }

    public uint ReadRegister(uint reg)
    {
        var response = SendCommand($"p{(reg).ToHex(prefix: false)}");
        return ParseLittleEndianHex(response);
    }

    public uint[] ReadRegisters()
    {
        var registers = SendCommand("g");
        var length = registers.Length / 8;
        if (registers.Length % 8 != 0)
        {
            throw new InvalidDataException();
        }

        var regs = new uint[length];

        for (int i = 0; i < length; i++)
        {
            regs[i] = ParseLittleEndianHex(registers.AsSpan(i * 8, 8));
        }

        return regs;
    }

    public void WriteRegister(uint reg, uint value)
    {
        var response = SendCommand($"P{reg.ToHex(prefix: false)}={BytesToHexString(BitConverter.GetBytes(value))}");
        if (response != "OK")
        {
            throw new InvalidOperationException($"Response is not OK - {response}");
        }
    }

    public string Monitor(string command)
    {
        return SendCommand($"qRcmd,{BytesToHexString(Encoding.ASCII.GetBytes(command))}");
    }

    public byte[] ReadMemory(uint address, uint length)
    {
        var hex = SendCommand($"m{address.ToHex(prefix: false)},{length.ToHex(prefix: false)}");
        return HexStringToBytes(hex);
    }

    public void WriteMemory(uint address, byte[] data)
    {
        var result = SendCommand($"M{address.ToHex(prefix: false)},{((uint)data.Length).ToHex(prefix: false)}:{BytesToHexString(data)}");
    }

    public void Step()
    {
        SendCommand("vCont;s:1;c", r =>
        {
            // Ignore all messages and wait for information that the stub is not running.
            return r.StartsWith("T05");
        });
    }

    // TODO: This should be in some shared library
    private static uint ParseLittleEndianHex(ReadOnlySpan<char> hex)
    {
        // TODO: Isn't this too complicated?
        Span<byte> bytes = stackalloc byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Slice(i * 2, 2).ToString(), 16);
        }
        return BitConverter.ToUInt32(bytes);
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

    private static string BytesToHexString(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    private static ulong ParseGdbHexAsULong(string hex)
    {
        // GDB register values are often sent byte-by-byte (e.g., 0A000000 for 10).
        // We need to parse pairs.
        byte[] data = HexStringToBytes(hex);

        // Pad to 8 bytes for ULong conversion if necessary
        if (data.Length < 8)
        {
            byte[] padded = new byte[8];
            Array.Copy(data, padded, data.Length);
            data = padded;
        }

        // Assume Little Endian (Standard for x86/ARM GDB Stubs)
        return BitConverter.ToUInt64(data, 0);
    }
}
