using Venture;
using Venture.Processor;

namespace Tests;

public class TestsFromRiscof
{
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
            var parts = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            TestDisplayName = $"{parts[^5]} {parts[^3].Substring(0, parts[^3].Length - 2)} ";
            this.path = path;
        }

        public object?[] GetData()
        {
            return [path];
        }
    }

    public static IEnumerable<ITheoryDataRow> GetData()
    {
        //"rv32i_m\\C\\add-01.S\\ref\\ref.elf"
        return Directory.EnumerateFiles("rv32i_m", "ref.elf", SearchOption.AllDirectories).Order()
            .Select(x => new ComplianceTestRow(x));
    }

    [Theory]
    [MemberData(nameof(GetData))]
    public void All(string path)
    {
        // ~/riscof/riscof_work/riscv-test-suite/rv32i_m/I/src/add-01.S/dut$ riscv32-unknown-elf-objdump -D my.elf > my.dump
        int maxSteps = 10_000;
        var isRunning = true;

        var m = new Memory(path, 0x80000000, 1024 * 1024 * 5);

        m.OnWrite += (s, a) =>
        {
            // ToHost address
            if (a.Address == m.Symbols["tohost"])
            {
                var data = BitConverter.ToUInt32(a.Data);
                Assert.Equal((uint)1, data);
                isRunning = false;
            }
        };

        var e = new Processor(m);

        e.EBreak += (s, a) =>
        {
            isRunning = false;
        };

        int i = 0;
        while (isRunning && i <= maxSteps)
        {
            e.Step();
            i++;
        }

        Assert.True(i <= maxSteps);

        var signature = File.ReadAllLines(path.Replace("ref.elf", "Reference-sail_c_simulator.signature"));

        var sigAddress = m.Symbols["begin_signature"];
        foreach (var line in signature)
        {
            var sigByte = m.ReadWord(sigAddress).ToHex();
            Assert.Equal($"0x{line}", sigByte);
            sigAddress += 4;
        }
    }
}
