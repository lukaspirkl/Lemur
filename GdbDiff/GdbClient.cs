using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace GdbDiff;

/// <summary>
/// A C# Client for the GDB Remote Serial Protocol (RSP).
/// </summary>
public class GdbClient : IDisposable
{
    private TcpClient _client;
    private NetworkStream _stream;
    private StreamReader _reader;
    private StreamWriter _writer;
    private readonly object _lock = new object();

    public bool IsConnected => _client != null && _client.Connected;

    /// <summary>
    /// Connects to a GDB Stub (e.g., OpenOCD, QEMU, gdbserver) over TCP.
    /// </summary>
    public void Connect(string host, int port)
    {
        if (IsConnected) Disconnect();

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

    public void Disconnect()
    {
        _stream?.Close();
        _client?.Close();
        _client = null;
    }

    public void Dispose()
    {
        Disconnect();
    }

    #region Core Protocol Implementation

    /// <summary>
    /// Sends a command packet, handles ACK/NACK, and waits for the response packet.
    /// </summary>
    /// <param name="commandData">The raw command string (e.g., "g", "m100,20")</param>
    /// <returns>The response payload.</returns>
    public string SendCommand(string commandData)
    {
        lock (_lock)
        {
            // 1. Construct the packet: $data#checksum
            string packet = FormatPacket(commandData);

            // 2. Send loop (handle NACKs)
            while (true)
            {
                WriteRaw(packet);

                // 3. Read immediate acknowledgment (+ or -)
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
                    // Some stubs might send output (O packet) or stop reply directly if we are out of sync.
                    // For this simple client, we treat it as a protocol violation.
                    throw new Exception($"[GDB] Protocol Error: Expected ACK (+), got '{ack}'");
                }
            }

            // 4. Read the response packet
            return ReadResponsePacket();
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

    /// <summary>
    /// Reads a standard packet from the stream.
    /// Format: $payload#checksum
    /// </summary>
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

    #endregion

    #region High-Level Commands

    /// <summary>
    /// Reads memory at the specified address.
    /// Protocol: m addr,length
    /// </summary>
    public byte[] ReadMemory(ulong address, int length)
    {
        string cmd = $"m{address:x},{length:x}";
        string resp = SendCommand(cmd);

        if (resp.StartsWith("E"))
            throw new Exception($"ReadMemory Failed. Error: {resp}");

        return HexStringToBytes(resp);
    }

    /// <summary>
    /// Writes memory at the specified address.
    /// Protocol: M addr,length:hex-data
    /// </summary>
    public void WriteMemory(ulong address, byte[] data)
    {
        string hexData = BytesToHexString(data);
        string cmd = $"M{address:x},{data.Length:x}:{hexData}";
        string resp = SendCommand(cmd);

        if (resp != "OK")
            throw new Exception($"WriteMemory Failed. Response: {resp}");
    }

    /// <summary>
    /// Reads a specific register.
    /// Protocol: p n
    /// </summary>
    public ulong ReadRegister(int registerIndex)
    {
        string cmd = $"p{registerIndex:x}";
        string resp = SendCommand(cmd);

        if (resp.StartsWith("E"))
            throw new Exception($"ReadRegister Failed. Error: {resp}");

        // GDB sends register data in target byte order (usually little endian for x86/ARM)
        // but as a raw hex string. 
        // Example: 0x1234 -> sent as "34120000" (if 32 bit).

        // For simplicity, we assume we want to parse it as a number.
        // Note: Actual implementation depends on architecture bit-width.
        return ParseGdbHexAsULong(resp);
    }

    /// <summary>
    /// Writes a specific register.
    /// Protocol: P n=val
    /// </summary>
    public void WriteRegister(int registerIndex, ulong value, int byteWidth = 4)
    {
        // Convert value to hex string in target byte order (Little Endian assumed)
        byte[] bytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian) Array.Reverse(bytes); // Host is Big, convert to LE? 
                                                                // Actually, GDB expects bytes in target order. Let's assume target is LE.

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < byteWidth; i++)
            sb.Append(bytes[i].ToString("x2"));

        string cmd = $"P{registerIndex:x}={sb}";
        string resp = SendCommand(cmd);

        if (resp != "OK")
            throw new Exception($"WriteRegister Failed. Response: {resp}");
    }

    /// <summary>
    /// Single Step instruction.
    /// Protocol: s [addr]
    /// </summary>
    public string Step()
    {
        // 's' does not return OK immediately. It returns a Stop Reply Packet (T signal...)
        // when the step is done.
        return SendCommand("vCont;s");
    }

    /// <summary>
    /// Continue execution.
    /// Protocol: c [addr]
    /// </summary>
    public string Continue()
    {
        // 'c' returns when a breakpoint is hit or execution stops.
        return SendCommand("c");
    }

    /// <summary>
    /// Sends an interrupt (Ctrl+C) 0x03 byte to halt the target.
    /// </summary>
    public void SendInterrupt()
    {
        lock (_lock)
        {
            _stream.WriteByte(0x03);
            // We expect a stop reply packet after this, handled by the next Read
        }
    }

    #endregion

    #region Utilities

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

    #endregion
}
