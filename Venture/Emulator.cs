namespace Venture;

public interface IEmulator
{
    IMemory Memory { get; }
    Registers Registers { get; }
    Dictionary<ushort, uint> CSR { get; }
    uint PC { get; set; }
    public void AddInstructionSet(Action<IInstructionSetBuider> factory);
}

public class Emulator : IEmulator
{
    private readonly InstructionSetCollection instructionSetCollection = new InstructionSetCollection();

    public IMemory Memory { get; }
    public Registers Registers { get; }
    public Dictionary<ushort, uint> CSR { get; } = new Dictionary<ushort, uint>();

    private bool m_IsPCModified = false;
    private uint m_PC;

    public uint PC
    {
        get { return m_PC; }
        set
        {
            m_IsPCModified = true;
            m_PC = value;
        }
    }

    public Emulator(IMemory memory)
    {
        Memory = memory;
        Registers = new Registers();
        PC = memory.InitialPC;
    }

    public void Step()
    {
        var instruction = Memory.ReadWord(PC);
        Console.WriteLine($"PC: {PC.ToHex()} Instruction: {instruction.ToHex()} {instruction.ToBin()}");
        //Console.WriteLine($"registers[5] {registers[5].ToHex()}");

        uint stepSize = 4;

        if (TryDecompress(ref instruction))
        {
            stepSize = 2;
        }
        
        m_IsPCModified = false;
        instructionSetCollection.Execute(instruction);
        if (!m_IsPCModified)
        {
            PC = PC + stepSize;
        }
        
    }

    private bool TryDecompress(ref uint instruction)
    {
        const uint nop = 0b0000_0000_0000_0000_0000_0000_0001_0011;

        uint op = instruction.ExtractBits(0, 2);
        
        if (op == 0b11)
        {
            return false;
        }

        if (op == 0b01)
        {
            var f = new CIInstructionFormat(instruction);
            if (f.funct3 == 0b000)
            {
                if (f.rd == 0)
                {
                    // Reserved for hints - no-op
                    instruction = nop;
                    return true;
                }

                // C.ADDI
                // TODO: verify
                instruction = 0b001_0011;
                instruction |= f.rd << 7;
                instruction |= f.rd << 15;
                instruction |= (uint)f.imm << 20;
                return true;
            }
            if (f.funct3 == 0b010)
            {
                if (f.rd == 0)
                {
                    // Reserved for hints - no-op
                    instruction = nop;
                    return true;
                }

                // C.LI
                instruction = 0b001_0011;
                instruction |= f.rd << 7;
                instruction |= (uint)f.imm << 20;
                return true;
            }
            if (f.funct3 == 0b011)
            {
                if (f.rd == 0)
                {
                    // Reserved for hints - no-op
                    // TODO: implement
                    instruction = nop;
                    return true;
                }
                if (f.rd == 2)
                {
                    // C.ADDI16SP
                    instruction = nop;
                    return true;
                }

                // C.LUI
                // TODO: verify
                instruction = 0b011_0111;
                instruction |= f.rd << 7;
                instruction |= (uint)f.imm << 12;
                return true;
            }
            if ((ushort)instruction == 0b0000_0000_0000_0001)
            {
                // C.NOP
                instruction = nop;
                return true;
            }
        }

        throw new ArgumentException($"Unknown compressed instruction {instruction.ToHex()}");
    }

    public void AddInstructionSet(Action<IInstructionSetBuider> factory)
    {
        factory(instructionSetCollection);
    }
}

public class CIInstructionFormat
{
    public uint op { get; }
    public int imm { get; }
    public uint rd { get; }
    public uint funct3 { get; }
    public CIInstructionFormat(uint instruction)
    {
        op = instruction.ExtractBits(0, 2);

        var imm5 = instruction.ExtractBits(12, 1);
        imm = (int)(instruction.ExtractBits(2, 5) | (imm5 << 5));
        if (imm5 == 1)
        {
            // Extend the sign bit
            imm |= unchecked((int)0xFFFF_FFFF << 6);
        }

        rd = instruction.ExtractBits(7, 5);
        funct3 = instruction.ExtractBits(13, 3);
    }
}