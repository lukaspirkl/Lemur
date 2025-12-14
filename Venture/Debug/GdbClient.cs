using System.Net.Sockets;
using System.Text;
using Venture.Processor;

namespace Venture.Debug;

public class RP2350GDB : IDebuggable, IDisposable
{
    private readonly GdbClient gdbClient;

    public RP2350GDB(string host, int port)
    {
        gdbClient = new GdbClient(host, port);
        Registers = new GdbRegisters(gdbClient);
    }

    public IIndexable<uint> Registers { get; }

    public void Dispose()
    {
        gdbClient.Dispose();
    }

    public byte[] MemoryRead(uint address, int count)
    {
        return gdbClient.ReadMemory(address, (uint)count);
    }

    public void MemoryWrite(uint address, byte[] data)
    {
        gdbClient.WriteMemory(address, data);
    }

    public void Run()
    {
        throw new NotImplementedException();
    }

    public void Step()
    {
        gdbClient.Step();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }


    class GdbRegisters : IIndexable<uint>
    {
        private readonly GdbClient gdbClient;

        public GdbRegisters(GdbClient gdbClient)
        {
            this.gdbClient = gdbClient;
        }

        public uint this[uint index]
        {
            get
            {
                var regs = gdbClient.ReadRegisters();
                return regs[index];
            }
            set
            {
                gdbClient.WriteRegister(index, value);
            }
        }

        public int Length => 33;
    }
}

public class GdbClient : IDisposable
{
    private TcpClient _client;
    private NetworkStream _stream;
    private StreamReader _reader;
    private StreamWriter _writer;
    private readonly object _lock = new object();

    public bool IsConnected => _client != null && _client.Connected;

    public GdbClient(string host, int port)
    {
        Console.WriteLine($"[GDB] Connecting to {host}:{port}...");
        _client = new TcpClient();
        _client.Connect(host, port);
        _stream = _client.GetStream();

        // Using basic ASCII/binary reading
        _reader = new StreamReader(_stream, Encoding.ASCII);
        _writer = new StreamWriter(_stream, Encoding.ASCII) { AutoFlush = true };

        // Turn off Nagle's algorithm for lower latency
        _client.NoDelay = true;

        // Initial handshake usually involves disabling AckMode in modern GDB, 
        // but for a basic client, we will assume standard AckMode is active.
        // We verify connection by sending a halt reason query.
        string response = SendCommand("?");
        Console.WriteLine($"[GDB] Connected. Initial State: {response}");
    }

    public void Dispose()
    {
        _stream.Close();
        _stream.Dispose();
        _client.Close();
        _client.Dispose();
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
        lock (_lock)
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
        _stream.Write(bytes, 0, bytes.Length);
    }

    private char ReadChar()
    {
        int b = _stream.ReadByte();
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
