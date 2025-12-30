namespace Venture;

public sealed class Register32
{
    private uint resetValue;
    private uint value;

    private readonly List<IField> fields = new();

    private Action<uint>? writeSideEffect;
    private Func<uint>? readOverride;

    public Register32(uint resetValue = 0)
    {
        this.resetValue = resetValue;
        value = resetValue;
    }

    public Register32 Field<T>(
        int lsb,
        int width,
        Func<T> getter,
        Action<T>? setter = null)
        where T : unmanaged, Enum
    {
        fields.Add(new EnumField<T>(lsb, width, getter, setter));
        return this;
    }

    public Register32 Field(
        int lsb,
        int width,
        Func<uint> getter,
        Action<uint>? setter = null)
    {
        fields.Add(new UIntField(lsb, width, getter, setter));
        return this;
    }

    public Register32 OnWrite(Action<uint> action)
    {
        writeSideEffect = action;
        return this;
    }

    public Register32 OnRead(Func<uint> action)
    {
        readOverride = action;
        return this;
    }

    public uint Read()
    {
        if (readOverride != null)
            return readOverride();

        uint result = value;
        foreach (var f in fields)
            result = f.Encode(result);
        return result;
    }

    public void Write(uint data)
    {
        foreach (var f in fields)
            f.Decode(data);

        writeSideEffect?.Invoke(data);
        value = data;
    }

    public void Reset() => value = resetValue;
}

interface IField
{
    uint Encode(uint regValue);
    void Decode(uint regValue);
}

sealed class UIntField : IField
{
    private readonly int lsb, width;
    private readonly uint mask;
    private readonly Func<uint> getter;
    private readonly Action<uint>? setter;

    public UIntField(int lsb, int width, Func<uint> getter, Action<uint>? setter)
    {
        this.lsb = lsb;
        this.width = width;
        this.getter = getter;
        this.setter = setter;
        mask = ((1u << width) - 1u) << lsb;
    }

    public uint Encode(uint reg)
    {
        reg &= ~mask;
        reg |= (getter() << lsb) & mask;
        return reg;
    }

    public void Decode(uint reg)
    {
        if (setter == null) return;
        setter((reg & mask) >> lsb);
    }
}

sealed class EnumField<T> : IField where T : unmanaged, Enum
{
    private readonly UIntField inner;

    public EnumField(int lsb, int width, Func<T> getter, Action<T>? setter)
    {
        inner = new UIntField(
            lsb,
            width,
            () => Convert.ToUInt32(getter()),
            setter == null ? null : v => setter((T)Enum.ToObject(typeof(T), v))
        );
    }

    public uint Encode(uint reg) => inner.Encode(reg);
    public void Decode(uint reg) => inner.Decode(reg);
}
