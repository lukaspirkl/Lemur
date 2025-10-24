namespace Venture;

internal class Program
{
    private static void Main(string[] args)
    {
        var e = new Emulator("../../../../Tests/bin/Debug/net9.0/rv32ui-p/rv32ui-p-add");
        bool isRunning = true;

        e.ECall += (s, a) =>
        {
            isRunning = false;
        };

        while (isRunning)
        {
            e.ExecuteInstruction();
        }
    }
}
