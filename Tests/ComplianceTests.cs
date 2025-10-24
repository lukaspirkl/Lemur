using Venture;

[assembly: CaptureConsole]

namespace Tests;

public class ComplianceTests
{
    [Fact(Timeout = 1000)]
    public void Add()
    {
        Run("rv32ui-p/rv32ui-p-add", 500);
    }

    [Fact(Timeout = 1000)]
    public void Addi()
    {
        Run("rv32ui-p/rv32ui-p-addi", 500);
    }

    [Fact(Timeout = 1000)]
    public void And()
    {
        Run("rv32ui-p/rv32ui-p-and", 500);
    }

    private void Run(string path, int steps)
    {
        var e = new Emulator(path);

        var isRunning = true;

        e.ECall += (s, a) =>
        {
            isRunning = false;
            Assert.Equal((uint)93, a.ServiceNumber);
            Assert.Equal((uint)0, a.Argument);
        };

        int i = 0;
        while (isRunning && i <= steps)
        {
            e.ExecuteInstruction();
            i++;
        }

        Assert.Equal(500, steps);
    }
}
