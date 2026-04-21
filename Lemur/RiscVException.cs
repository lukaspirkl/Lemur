using System;

namespace Lemur;

public class RiscVException : ApplicationException
{
    public ExceptionCause Cause { get; }
    public uint TrapValue { get; }

    public RiscVException(ExceptionCause cause, uint trapValue, string message) : base(message)
    {
        Cause = cause;
        TrapValue = trapValue;
    }
}

public enum ExceptionCause : uint
{
    InstructionAddressMisaligned = 0,
    InstructionAccessFault = 1,
    IllegalInstruction = 2,
    Breakpoint = 3,
    LoadAddressMisaligned = 4,
    LoadAccessFault = 5,
    StoreAddressMisaligned = 6,
    StoreAccessFault = 7,
    EnvironmentCallFromMMode = 11,
}
