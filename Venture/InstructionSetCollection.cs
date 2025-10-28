using Venture.InstructionFormats;

namespace Venture;

public interface IInstructionSetBuider
{
    IInstructionSetBuiderOpcode<T> Opcode<T>(uint opcode, Func<uint, T> formatFactory) where T : InstructionFormatBase;
    void Opcode<T>(uint opcode, Func<uint, T> formatFactory, string mnemonic, Action<T> instruction) where T : InstructionFormatBase;
}

public interface IInstructionSetBuiderOpcode<T> where T : InstructionFormatBase
{
    IInstructionSetBuiderOpcode<T> Funct3(uint funct3, string mnemonic, Action<T> operation);
}

public class InstructionSetCollection : IInstructionSetBuider
{
    private abstract record Value;
    private record Funct3Operations(Dictionary<uint, FuncValue> Funct3) : Value;
    private record FuncValue(string Mnemonic, Action<uint> Operation) : Value;

    private readonly Dictionary<uint, Value> instructions = new();

    public IInstructionSetBuiderOpcode<T> Opcode<T>(uint opcode, Func<uint, T> formatFactory) where T : InstructionFormatBase
    {
        if (instructions.TryGetValue(opcode, out var value))
        {
            if (value is Funct3Operations ops)
            {
                return new InstructionSetBuiderOpcode<T>(ops, formatFactory);
            }
            throw new ArgumentException($"Opcode {opcode.ToBin(7)} already added", nameof(opcode));
        }
        else
        {
            var ops = new Funct3Operations(new Dictionary<uint, FuncValue>());
            instructions.Add(opcode, ops);
            return new InstructionSetBuiderOpcode<T>(ops, formatFactory);
        }
    }

    public void Opcode<T>(uint opcode, Func<uint, T> formatFactory, string mnemonic, Action<T> operation) where T : InstructionFormatBase
    {
        instructions.Add(opcode, new FuncValue(mnemonic, instruction => operation(formatFactory(instruction))));
    }

    private class InstructionSetBuiderOpcode<T> : IInstructionSetBuiderOpcode<T> where T : InstructionFormatBase
    {
        private readonly Funct3Operations operations;
        private readonly Func<uint, T> formatFactory;

        public InstructionSetBuiderOpcode(Funct3Operations operations, Func<uint, T> formatFactory)
        {
            this.operations = operations;
            this.formatFactory = formatFactory;
        }

        public IInstructionSetBuiderOpcode<T> Funct3(uint funct3, string mnemonic, Action<T> operation)
        {
            operations.Funct3.Add(funct3, new FuncValue(mnemonic, instruction => operation(formatFactory(instruction))));
            return this;
        }
    }

    public void Execute(uint instruction)
    {
        var opcode = instruction.ExtractBits(0, 7);
        if (!instructions.TryGetValue(opcode, out var value))
        {
            throw new ArgumentException($"Unknown opcode {opcode.ToBin(7)} in instruction {instruction.ToHex()}", nameof(instruction));
        }

        if (value is FuncValue funcValue)
        {
            funcValue.Operation(instruction);
            return;
        }
        
        if (value is Funct3Operations funct3Ops)
        {
            var funct3 = instruction.ExtractBits(12, 3);
            if (!funct3Ops.Funct3.TryGetValue(funct3, out var funct3Value))
            {
                throw new ArgumentException($"Unknown funct3 {funct3.ToBin(3)} in opcode {opcode.ToBin(7)} in instruction {instruction.ToHex()}", nameof(instruction));
            }

            funct3Value.Operation(instruction);
            return;
        }

        throw new ArgumentException($"Unknown registration for instruction {instruction.ToHex()}", nameof(instruction));
    }
}
