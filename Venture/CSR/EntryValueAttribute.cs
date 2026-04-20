namespace Venture.Csr;

[AttributeUsage(AttributeTargets.Property)]
public class EntryValueAttribute : Attribute
{
    public string Bits { get; }
    public string Description { get; }

    public EntryValueAttribute(string bits, string description)
    {
        Bits = bits;
        Description = description;
    }
}
