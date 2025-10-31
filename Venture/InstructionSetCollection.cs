using System;
using System.Reflection.Emit;
using Venture.InstructionFormats;

namespace Venture;

public interface IInstructionSetBuider
{
    IInstructionSetBuilderOpcode<T> Opcode<T>(uint opcode, Func<uint, T> formatFactory) where T : InstructionFormatBase;
    void Opcode<T>(uint opcode, Func<uint, T> formatFactory, string mnemonic, Action<T> instruction) where T : InstructionFormatBase;
}

public interface IInstructionSetBuilderOpcode<T> where T : InstructionFormatBase
{
    IInstructionSetBuilderOpcode<T> Funct3(uint funct3, string mnemonic, Action<T> operation);
    IInstructionSetBuilderOpcode<T> Funct3(uint funct3, Action<IInstructionSetBuilderFunct3<T>> builder);
}

public interface IInstructionSetBuilderFunct3<T> where T : InstructionFormatBase
{
    IInstructionSetBuilderFunct3<T> Funct7(uint funct7, string mnemonic, Action<T> operation);
}

public class InstructionSetCollection : IInstructionSetBuider
{
    private abstract record OpcodeValue;
    private record OpcodeOperationValue(string Mnemonic, Action<uint> Operation) : OpcodeValue;
    private record OpcodeFunct3Value(Dictionary<uint, Funct3Value> Funct3) : OpcodeValue;
    private abstract record Funct3Value;
    private record Funct3OperationValue(string Mnemonic, Action<uint> Operation) : Funct3Value;
    private record Funct3Funct7Value(Dictionary<uint, Funct7OperationValue> Funct7) : Funct3Value;
    private record Funct7OperationValue(string Mnemonic, Action<uint> Operation);

    private readonly Dictionary<uint, OpcodeValue> instructions = new();

    public IInstructionSetBuilderOpcode<T> Opcode<T>(uint opcode, Func<uint, T> formatFactory) where T : InstructionFormatBase
    {
        if (instructions.TryGetValue(opcode, out var value))
        {
            if (value is OpcodeFunct3Value ops)
            {
                return new InstructionSetBuiderOpcode<T>(ops, formatFactory);
            }
            throw new ArgumentException($"Opcode {opcode.ToBin(7)} already added", nameof(opcode));
        }
        else
        {
            var ops = new OpcodeFunct3Value(new Dictionary<uint, Funct3Value>());
            instructions.Add(opcode, ops);
            return new InstructionSetBuiderOpcode<T>(ops, formatFactory);
        }
    }

    public void Opcode<T>(uint opcode, Func<uint, T> formatFactory, string mnemonic, Action<T> operation) where T : InstructionFormatBase
    {
        instructions.Add(opcode, new OpcodeOperationValue(mnemonic, instruction => operation(formatFactory(instruction))));
    }

    private class InstructionSetBuiderOpcode<T> : IInstructionSetBuilderOpcode<T> where T : InstructionFormatBase
    {
        private readonly OpcodeFunct3Value operations;
        private readonly Func<uint, T> formatFactory;

        public InstructionSetBuiderOpcode(OpcodeFunct3Value operations, Func<uint, T> formatFactory)
        {
            this.operations = operations;
            this.formatFactory = formatFactory;
        }

        public IInstructionSetBuilderOpcode<T> Funct3(uint funct3, string mnemonic, Action<T> operation)
        {
            operations.Funct3.Add(funct3, new Funct3OperationValue(mnemonic, instruction => operation(formatFactory(instruction))));
            return this;
        }

        public IInstructionSetBuilderOpcode<T> Funct3(uint funct3, Action<IInstructionSetBuilderFunct3<T>> funct7)
        {
            if (operations.Funct3.TryGetValue(funct3, out var value))
            {
                if (value is Funct3Funct7Value ops)
                {
                    funct7(new InstructionSetBuilderFunct3<T>(ops, formatFactory));
                    return this;
                }
                throw new ArgumentException($"Funct3 {funct3.ToBin(3)} already added", nameof(funct3));
            }
            else
            {
                var ops = new Funct3Funct7Value(new Dictionary<uint, Funct7OperationValue>());
                operations.Funct3.Add(funct3, ops);
                funct7(new InstructionSetBuilderFunct3<T>(ops, formatFactory));
                return this;
            }
        }
    }

    private class InstructionSetBuilderFunct3<T> : IInstructionSetBuilderFunct3<T> where T : InstructionFormatBase
    {
        private readonly Funct3Funct7Value operations;
        private readonly Func<uint, T> formatFactory;

        public InstructionSetBuilderFunct3(Funct3Funct7Value operations, Func<uint, T> formatFactory)
        {
            this.operations = operations;
            this.formatFactory = formatFactory;
        }

        public IInstructionSetBuilderFunct3<T> Funct7(uint funct7, string mnemonic, Action<T> operation)
        {
            operations.Funct7.Add(funct7, new Funct7OperationValue(mnemonic, instruction => operation(formatFactory(instruction))));
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

        if (value is OpcodeOperationValue funcValue)
        {
            funcValue.Operation(instruction);
            return;
        }
        
        if (value is OpcodeFunct3Value funct3Ops)
        {
            var funct3 = instruction.ExtractBits(12, 3);
            if (!funct3Ops.Funct3.TryGetValue(funct3, out var funct3Value))
            {
                throw new ArgumentException($"Unknown funct3 {funct3.ToBin(3)} in opcode {opcode.ToBin(7)} in instruction {instruction.ToHex()}", nameof(instruction));
            }

            if (funct3Value is Funct3OperationValue funct3Operation)
            {
                funct3Operation.Operation(instruction);
            }

            if (funct3Value is Funct3Funct7Value funct3Funct7Operations)
            {
                var funct7 = instruction.ExtractBits(25, 7);
                if (!funct3Funct7Operations.Funct7.TryGetValue(funct7, out var funct7Value))
                {
                    throw new ArgumentException($"Unknown funct7 {funct7.ToBin(7)} in funct3 {funct3.ToBin(3)} in opcode {opcode.ToBin(7)} in instruction {instruction.ToHex()}", nameof(instruction));
                }

                if (funct7Value is Funct7OperationValue funct7Operation)
                {
                    funct7Operation.Operation(instruction);
                }
            }

            return;
        }

        throw new ArgumentException($"Unknown registration for instruction {instruction.ToHex()}", nameof(instruction));
    }
}
