namespace Hyper.Compiler.Symbols;

public sealed class VariableSymbol : Symbol
{
    public VariableSymbol(string name, Type type, bool isReadOnly) : base(name)
    {
        Type = type;
        IsReadOnly = isReadOnly;
    }

    public override SymbolKind Kind       => SymbolKind.Variable;
    public          Type       Type       { get; }
    public          bool       IsReadOnly { get; }
}
