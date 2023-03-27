namespace Hyper.Compiler.Symbols;

public sealed class ParameterSymbol : VariableSymbol
{
    public ParameterSymbol(string name, TypeSymbol type)
        : base(name, type, isReadOnly: true) { }

    public override SymbolKind Kind => SymbolKind.Parameter;
}
