namespace Hyper.Core.Symbols;

public sealed class ParameterSymbol : LocalVariableSymbol
{
    public ParameterSymbol(string name, TypeSymbol type, int ordinal)
        : base(name, type, isReadOnly: true)
    {
        Ordinal = ordinal;
    }

    public override SymbolKind Kind    => SymbolKind.Parameter;
    public          int        Ordinal { get; }
}
