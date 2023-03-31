namespace Hyper.Compiler.Symbols;

public abstract class VariableSymbol : Symbol
{
    public VariableSymbol(string name, TypeSymbol type, bool isReadOnly) : base(name)
    {
        Type = type;
        IsReadOnly = isReadOnly;
    }

    public TypeSymbol Type       { get; }
    public bool       IsReadOnly { get; }
}
