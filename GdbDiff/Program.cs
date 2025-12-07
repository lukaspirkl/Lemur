namespace GdbDiff;

class Program
{
    

    static void Main(string[] args)
    {
        try
        {
            var logFile = "output.log";
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
            using var stream = File.OpenWrite(logFile);
            using var streamWriter = new StreamWriter(stream);

            using var gdbReal = new GdbClient("127.0.0.1", 50000);

            gdbReal.Monitor("reset halt");
            //WipeRam(gdbReal);


            using var gdbEmu = new GdbClient("127.0.0.1", 3333);

            uint count = 0;
            while (Compare(gdbReal, gdbEmu, streamWriter))
            {
                Console.WriteLine($"Count {count++}");
                gdbReal.Step();
                gdbEmu.Step();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
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

    static bool Compare(GdbClient gdbReal, GdbClient gdbEmu, StreamWriter streamWriter)
    {
        var regsReal = gdbReal.ReadRegisters();
        var regsSim = gdbEmu.ReadRegisters();

        for (int i = 0; i < regsReal.Length; i++)
        {
            streamWriter.Write($"{i}: {regsReal[i].ToHex()} ");
        }
        streamWriter.WriteLine();
        streamWriter.WriteLine();


        for (int i = 0; i < regsReal.Length; i++)
        {
            if (regsReal[i] != regsSim[i])
            {
                var diffLog = $"DIFF in register {i} - real:{regsReal[i].ToHex()} sim: {regsSim[i].ToHex()}";
                Console.WriteLine(diffLog);
                streamWriter.WriteLine(diffLog);

                if (regsReal.Last() == 0x7472) // This is bootrom address that is getting value from TRNG
                {
                    Console.Write("AUTOMATIC REG REPLACEMENT");
                    gdbEmu.WriteRegister((uint)i, regsReal[i]);
                }
                else
                {
                    Console.Write("[S] set value to emulator and continue | [ANY] exit");
                    if (Console.ReadKey().KeyChar == 's')
                    {
                        gdbEmu.WriteRegister((uint)i, regsReal[i]);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        var pcLog = $"PC - real:{regsReal.Last().ToHex()} sim: {regsSim.Last().ToHex()}";
        Console.WriteLine(pcLog);
        streamWriter.WriteLine(pcLog);

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
