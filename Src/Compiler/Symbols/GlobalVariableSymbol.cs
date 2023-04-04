namespace Hyper.Compiler.Symbols;

public sealed class GlobalVariableSymbol : VariableSymbol
{
    internal GlobalVariableSymbol(string name, TypeSymbol type, bool isReadOnly)
        : base(name, type, isReadOnly) { }

    public override SymbolKind Kind => SymbolKind.GlobalVariable;
}
