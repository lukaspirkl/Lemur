using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Venture.Debug;

namespace GdbDiff;

class Program
{
    private static readonly string logFile = "GdbDiff.clef";

    static void Main(string[] args)
    {
        if (File.Exists(logFile))
        {
            File.Delete(logFile);
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .WriteTo.Async(a => a.File(new CompactJsonFormatter(), logFile, restrictedToMinimumLevel: LogEventLevel.Debug))
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();

        try
        {
            using var gdbReal = new GdbClient("127.0.0.1", 50000);

            gdbReal.Monitor("reset halt");
            //WipeRam(gdbReal);


            using var gdbEmu = new GdbClient("127.0.0.1", 3333);

            uint count = 0;
            while (Compare(gdbReal, gdbEmu))
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
        uint address = 0x20000000;
        for (int i = 0; i < 512*2; i++)
        {
            gdb.WriteMemory(address, new byte[1024/2]);
            address += 1024/2;
        }
    }

    static bool Compare(GdbClient gdbReal, GdbClient gdbEmu)
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
                else
                {
                    //Log.Fatal("AUTOMATIC REG REPLACEMENT");
                    //gdbEmu.WriteRegister((uint)i, regsReal[i]);

                    Console.Write("[S] set value to emulator and continue | [ANY] exit");
                    while (true)
                    {
                        var keyChar = Console.ReadKey().KeyChar;
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
