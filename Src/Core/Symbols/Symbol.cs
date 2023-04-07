namespace Hyper.Core.Symbols;

public abstract class Symbol
{
    protected Symbol(string name)
    {
        Name = name;
    }

    public abstract SymbolKind Kind { get; }
    public          string     Name { get; }

    public void WriteTo(TextWriter writer) => SymbolPrinter.WriteTo(this, writer);

    public override string ToString()
    {
        using (var writer = new StringWriter())
        {
            WriteTo(writer);
            return writer.ToString();
        }
    }
}
