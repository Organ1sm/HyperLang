using Hyper.Core.Binder.Expr;

namespace Hyper.Core.Symbols;

public sealed class GlobalVariableSymbol : VariableSymbol
{
    internal GlobalVariableSymbol(string name, TypeSymbol type, bool isReadOnly, BoundConstant? constant)
        : base(name, type, isReadOnly, constant) { }

    public override SymbolKind Kind => SymbolKind.GlobalVariable;
}
