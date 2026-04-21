using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Lemur.Csr;

// ── Base ──────────────────────────────────────────────────────────────────────

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public abstract class CsrEntry
{
    public ushort Address { get; }
    public string Name    { get; }
    public string Group   { get; }

    protected CsrEntry(ushort address, string name, string group)
        => (Address, Name, Group) = (address, name, group);

    public event Action? Changed;

    protected abstract uint ReadCore();
    protected abstract void WriteCore(uint value);
    public virtual uint Peek() => ReadCore();

    public uint Read() => ReadCore();

    public void Write(uint value)
    {
        WriteCore(value);
        Changed?.Invoke();
    }

    protected void Set<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        Changed?.Invoke();
    }

    public IEnumerable<EntryValue> GetValues()
    {
        var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

        var list = new List<EntryValue>();

        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<EntryValueAttribute>();
            if (attr == null)
                continue;

            var rawValue = prop.GetValue(this);

            uint value;
            try
            {
                value = rawValue != null ? Convert.ToUInt32(rawValue) : 0;
            }
            catch (Exception)
            {
                throw new InvalidOperationException(
                    $"Cannot convert property {prop.Name} of type {prop.PropertyType} to uint");
            }

            list.Add(new EntryValue(
                prop.Name,
                attr.Bits,
                attr.Description,
                value
            ));
        }

        return list.OrderBy(e => e.Bits);
    }
}
