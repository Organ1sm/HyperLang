namespace Hyper.Compiler.Symbols;

public class VariableSymbol : Symbol
{
    public VariableSymbol(string name, TypeSymbol type, bool isReadOnly) : base(name)
    {
        Type = type;
        IsReadOnly = isReadOnly;
    }

    public override SymbolKind Kind       => SymbolKind.Variable;
    public          TypeSymbol Type       { get; }
    public          bool       IsReadOnly { get; }
}
