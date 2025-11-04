namespace Venture;

internal class Program
{
    private static void Main(string[] args)
    {

        bool isRunning = true;
        //"C:\Users\pirkl\Downloads\release\bare_metal\machine\paging_bare\rv_i\rv32i_0"
        var m = new Memory(@"Blink\KeySquareBlink.elf", 0x10000000, 1024 * 1024 * 2);
        //var m = new Memory(@"C:\Users\pirkl\Downloads\release\bare_metal\machine\paging_bare\rv_i\rv32i_0", 0x10000000, 1024 * 1024 * 2);
        var e = new Emulator(m);
        e.AddRV32I();

        // Start from the main - TODO: figure out how to get there and where I should really start
        e.PC = 0x10000124;

        while (isRunning)
        {
            e.Step();
        }
    }
}
