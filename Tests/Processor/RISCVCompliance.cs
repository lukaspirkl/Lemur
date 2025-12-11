using Microsoft.Extensions.Logging.Testing;
using Venture;
using Venture.Processor;

namespace Tests.Processor;

public class RISCVCompliance
{
    public class ComplianceTestRow : ITheoryDataRow
    {
        public bool? Explicit { get; set; }

        public string? Skip { get; set; }

        public string? TestDisplayName { get; set; }

        public int? Timeout { get; set; }

        public Dictionary<string, HashSet<string>>? Traits { get; set; }

        public string? Label { get; set; }

        public Type? SkipType { get; set; }

        public string? SkipUnless { get; set; }

        public string? SkipWhen { get; set; }

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
        return Directory.EnumerateFiles("rv32i_m", "my.elf", SearchOption.AllDirectories).Order()
            .Select(x => new ComplianceTestRow(x));
    }

    [Theory]
    [MemberData(nameof(GetData))]
    public void All(string path)
    {
        // ~/riscof/riscof_work/riscv-test-suite/rv32i_m/I/src/add-01.S/dut$ riscv32-unknown-elf-objdump -D my.elf > my.dump
        int maxSteps = 10_000;
        var isRunning = true;

        var ram = new Memory("ram", 0x80000000, 1024 * 1024 * 5, new FakeLogger<Memory>());
        ram.LoadElf(path);

        var m = new BusFabric([ram], new FakeLogger<BusFabric>());

        ram.OnWrite += (s, a) =>
        {
            // ToHost address
            if (a.Address == ram.Symbols["tohost"])
            {
                var data = BitConverter.ToUInt32(a.Data);
                Assert.Equal((uint)1, data);
                isRunning = false;
            }
        };

        var csr = new CSR(new FakeLogger<CSR>());

        var r = new Registers(new FakeLogger<Registers>());
        var e = new Hazard3Processor(m, new FakeLogger<Hazard3Processor>(), r, csr);

        // This is required for the hint tests. Machine Timer Interrupt Pending (MTIP) bit should be set to 1.
        // https://riscv-software-src.github.io/riscv-unified-db/manual/html/isa/isa_20240411/csrs/mip.html#mip-MTIP-def
        e.CSR.Set(0x344, 0x00000080);

        e.PC = 0x80000000;

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

        var signature = File.ReadAllLines(path.Replace("my.elf", "Reference-spike.signature"));

        var sigAddress = ram.Symbols["begin_signature"];
        foreach (var line in signature)
        {
            var sigByte = m.ReadWord(sigAddress).ToHex();
            Assert.Equal($"0x{line.ToUpper()}", sigByte);
            sigAddress += 4;
        }
    }
}
