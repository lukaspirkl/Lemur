using Microsoft.Extensions.Logging;

namespace Venture;

public sealed class Register32
{
    private readonly ILogger m_Logger;

    private uint m_ResetValue;
    private uint m_Value;

    private readonly List<IField> m_Fields = new();

    private Action<uint>? m_WriteSideEffect;
    private Func<uint>? m_ReadOverride;

    public string PeripheralName { get; }
    public string RegisterName { get; }

    public Register32(ILogger logger, string peripheralName, string registerName, uint resetValue = 0)
    {
        PeripheralName = peripheralName;
        RegisterName = registerName;
        m_ResetValue = resetValue;
        m_Value = resetValue;
        m_Logger = logger;
    }

    public Register32 Field<T>(int lsb, int width, Func<T> getter, Action<T>? setter = null) where T : unmanaged, Enum
    {
        m_Fields.Add(new EnumField<T>(lsb, width, getter, setter));
        return this;
    }

    public Register32 Field(int lsb, int width, Func<uint> getter, Action<uint>? setter = null)
    {
        m_Fields.Add(new UIntField(lsb, width, getter, setter));
        return this;
    }

    public Register32 Field(int lsb, Func<bool> getter, Action<bool>? setter = null)
    {
        m_Fields.Add(new BitField(lsb, getter, setter));
        return this;
    }

    public Register32 OnWrite(Action<uint> action)
    {
        m_WriteSideEffect = action;
        return this;
    }

    public Register32 OnRead(Func<uint> action)
    {
        m_ReadOverride = action;
        return this;
    }

    public uint Read()
    {
        m_Logger.LogTrace("Reading from {peripheral} register {register}", PeripheralName, RegisterName);

        if (m_ReadOverride != null)
        {
            return m_ReadOverride();
        }

        uint result = m_Value;

        foreach (var f in m_Fields)
        {
            result = f.Encode(result);
        }

        return result;
    }

    public void Write(uint data)
    {
        m_Logger.LogTrace("Writing to {peripheral} register {register} {data}", PeripheralName, RegisterName, data.ToHex());

        foreach (var f in m_Fields)
        {
            f.Decode(data);
        }

        m_WriteSideEffect?.Invoke(data);
        m_Value = data;
    }

    public void Reset() => m_Value = m_ResetValue;
}

interface IField
{
    uint Encode(uint regValue);
    void Decode(uint regValue);
}

sealed class BitField : IField
{
    private readonly uint m_Mask;
    private readonly Func<bool> m_Getter;
    private readonly Action<bool>? m_Setter;

    public BitField(int bit, Func<bool> getter, Action<bool>? setter)
    {
        m_Getter = getter;
        m_Setter = setter;
        m_Mask = 1u << bit;
    }

    public uint Encode(uint reg)
    {
        reg &= ~m_Mask;
        if (m_Getter())
        {
            reg |= m_Mask;
        }
        return reg;
    }

    public void Decode(uint reg)
    {
        if (m_Setter == null) return;
        m_Setter((reg & m_Mask) != 0);
    }
}

sealed class UIntField : IField
{
    private readonly int m_Lsb;
    private readonly uint m_Mask;
    private readonly Func<uint> m_Getter;
    private readonly Action<uint>? m_Setter;

    public UIntField(int lsb, int width, Func<uint> getter, Action<uint>? setter)
    {
        m_Lsb = lsb;
        m_Getter = getter;
        m_Setter = setter;
        m_Mask = ((1u << width) - 1u) << lsb;
    }

    public uint Encode(uint reg)
    {
        reg &= ~m_Mask;
        reg |= (m_Getter() << m_Lsb) & m_Mask;
        return reg;
    }

    public void Decode(uint reg)
    {
        if (m_Setter == null) return;
        m_Setter((reg & m_Mask) >> m_Lsb);
    }
}

sealed class EnumField<T> : IField where T : unmanaged, Enum
{
    private readonly UIntField m_Inner;

    public EnumField(int lsb, int width, Func<T> getter, Action<T>? setter)
    {
        m_Inner = new UIntField(
            lsb,
            width,
            () => Convert.ToUInt32(getter()),
            setter == null ? null : v => setter((T)Enum.ToObject(typeof(T), v))
        );
    }

    public uint Encode(uint reg) => m_Inner.Encode(reg);
    public void Decode(uint reg) => m_Inner.Decode(reg);
}
