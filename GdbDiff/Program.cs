using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Lemur.Debug;

namespace GdbDiff;

class Program
{
    private static readonly string m_LogFile = "GdbDiff.clef";

    static void Main(string[] args)
    {
        if (File.Exists(m_LogFile))
        {
            File.Delete(m_LogFile);
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .WriteTo.Async(a => a.File(new CompactJsonFormatter(), m_LogFile, restrictedToMinimumLevel: LogEventLevel.Debug))
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();

        try
        {
            using var gdbReal = new GdbClient("127.0.0.1", 50000);

            gdbReal.Monitor("reset halt");
            WipeRam(gdbReal);


            using var gdbEmu = new GdbClient("127.0.0.1", 3333);

            uint count = 0;
            while (Compare(gdbReal, gdbEmu, count))
            {
                Log.Information($"Count {count++}");
                gdbReal.Step();
                gdbEmu.Step();
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unexpected exception caused application crash.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    static void WipeRam(GdbClient gdb)
    {
        Log.Information("Wipe RAM with ones");
        uint address = 0x20000000;
        var data = new byte[1024 / 2];
        Array.Fill<byte>(data, 0xFF);
        for (int i = 0; i < 520*2; i++)
        {
            gdb.WriteMemory(address, data);
            address += 1024/2;
        }
    }

    static bool Compare(GdbClient gdbReal, GdbClient gdbEmu, uint count)
    {
        var regsReal = gdbReal.ReadRegisters();
        var regsSim = gdbEmu.ReadRegisters();

        for (int i = 0; i < regsReal.Length; i++)
        {
            Log.Debug($"{i}: {regsReal[i].ToHex()} ");
        }

        for (int i = 0; i < regsReal.Length; i++)
        {
            if (regsReal[i] != regsSim[i])
            {
                Log.Warning($"DIFF in register {i} - real:{regsReal[i].ToHex()} sim: {regsSim[i].ToHex()}");

                if (regsReal.Last() == 0x7472) // This is bootrom address that is getting value from TRNG
                {
                    Log.Information("AUTOMATIC REG REPLACEMENT (value from TRNG)");
                    gdbEmu.WriteRegister((uint)i, regsReal[i]);
                }
                //else if (count <= 118364)
                //{
                //    Log.Information("AUTOMATIC REG REPLACEMENT (instruction count less then limit)");
                //    gdbEmu.WriteRegister((uint)i, regsReal[i]);
                //}
                else
                {
                    Console.Write("[S] set value to emulator and continue | [X] exit");
                    while (true)
                    {
                        // Clear any buffered key presses
                        while (Console.KeyAvailable)
                        {
                            Console.ReadKey(intercept: true);
                        }

                        var keyChar = Console.ReadKey(intercept: true).KeyChar;
                        if (keyChar == 's')
                        {
                            Log.Information("USER REG REPLACEMENT");
                            gdbEmu.WriteRegister((uint)i, regsReal[i]);
                            break;
                        }
                        else if (keyChar == 'x')
                        {
                            return false;
                        }
                        else
                        {
                            Console.WriteLine($"Unknown char '{keyChar.ToString()}'");
                        }
                    }
                }
            }
        }

        Log.Information($"PC - real:{regsReal.Last().ToHex()} sim: {regsSim.Last().ToHex()}");

        //uint address = 0x20000000;
        //for (int i = 0; i < 512 * 4; i++)
        //{
        //    var memReal = gdbReal.ReadMemory(address, 1024 / 4);
        //    var memSim = gdbSim.ReadMemory(address, 1024 / 4);

        //    for (int j = 0; j < memReal.Length; j++)
        //    {
        //        if (memReal[j] != memSim[j])
        //        {
        //            Console.WriteLine($"DIFF in memory {j} - real:{memReal[j]} sim: {memSim[j]}");
        //            return false;
        //        }
        //    }

        //    address += 1024 / 2;
        //}

        return true;
    }
}
