using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Venture.Csr;

// ── Base ──────────────────────────────────────────────────────────────────────

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public abstract class CsrEntry
{
    public ushort Address { get; }
    public string Name    { get; }
    public string Group   { get; }

    protected CsrEntry(ushort address, string name, string group)
        => (Address, Name, Group) = (address, name, group);

    public abstract uint Read();
    public abstract void Write(uint value);
    public virtual uint Peek() => Read();

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
