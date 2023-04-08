namespace Hyper.Core.Symbols;

public class LocalVariableSymbol : VariableSymbol
{
    internal LocalVariableSymbol(string name, TypeSymbol type, bool isReadOnly)
        : base(name, type, isReadOnly) { }

    public override SymbolKind Kind => SymbolKind.LocalVariable;
}
