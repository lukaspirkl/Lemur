using Venture;

[assembly: CaptureConsole]

namespace Tests;

public class ComplianceTestRow : ITheoryDataRow
{
    public bool? Explicit { get; set; }

    public string? Skip { get; set; }

    public string? TestDisplayName { get; set; }

    public int? Timeout { get; set; }

    public Dictionary<string, HashSet<string>>? Traits { get; set; }

    private string path;

    public ComplianceTestRow(string path)
    {
        TestDisplayName = path.Split('-').Last();
        this.path = path;
    }

    public object?[] GetData()
    {
        return [path];
    }
}

public class ComplianceTests
{
    public static IEnumerable<ITheoryDataRow> GetData()
    {
        return Directory.EnumerateFiles("rv32ui-p")
            .Where(x => !x.Contains("."))
            .Select(x => new ComplianceTestRow(x));
    }

    [Theory]
    [MemberData(nameof(GetData))]
    public void All(string path)
    {
        int maxSteps = 2000;
        var m = new Memory(path, 0x80000000, 1024 * 128);
        var e = new Emulator(m);

        var isRunning = true;

        e.ECall += (s, a) =>
        {
            isRunning = false;
            Assert.Equal((uint)93, a.ServiceNumber);
            Assert.Equal((uint)0, a.Argument);
        };

        int i = 0;
        while (isRunning && i <= maxSteps)
        {
            e.ExecuteInstruction();
            i++;
        }

        Assert.True(i <= maxSteps);
    }
}
