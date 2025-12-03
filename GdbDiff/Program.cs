namespace GdbDiff;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== C# GDB Remote Client Demo ===");
        
        string host = "127.0.0.1";
        int port = 3333;

        try
        {
            using (var gdb = new GdbClient())
            {
                gdb.Connect(host, port);

                Console.WriteLine("NOTE: Connect call is commented out to prevent crashing without a live GDB stub.");
                Console.WriteLine("The code below demonstrates how to use the API.");

                if (gdb.IsConnected)
                {
                    ulong rax = gdb.ReadRegister(0);
                    Console.WriteLine($"Register 0 (RAX/EAX): 0x{rax:X}");

                    ulong addr = 0x0;
                    byte[] mem = gdb.ReadMemory(addr, 16);
                    Console.WriteLine($"Memory at 0x{addr:X}: {BitConverter.ToString(mem)}");

                    gdb.WriteMemory(addr, new byte[] { 0xCC });
                    Console.WriteLine("Wrote Breakpoint (0xCC) to memory.");

                    Console.WriteLine("Stepping...");
                    string stopReply = gdb.Step();
                    Console.WriteLine($"Target stopped. Reply: {stopReply}");
                 
                    
                    Console.WriteLine("Continuing...");
                    
                    stopReply = gdb.Continue();
                    Console.WriteLine($"Target stopped. Reply: {stopReply}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
}
