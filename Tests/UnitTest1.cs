using Venture;

[assembly: CaptureConsole]

namespace Tests;


public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var e = new Emulator("rv32ui-p/rv32ui-p-add");
        e.ExecuteInstruction();
        e.ExecuteInstruction();
    }
}
