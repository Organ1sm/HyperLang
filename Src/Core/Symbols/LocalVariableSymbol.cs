using Hyper.Core.Binder.Expr;

namespace Hyper.Core.Symbols;

public class LocalVariableSymbol : VariableSymbol
{
    internal LocalVariableSymbol(string name, TypeSymbol type, bool isReadOnly, BoundConstant? constant)
        : base(name, type, isReadOnly, constant) { }

    public override SymbolKind Kind => SymbolKind.LocalVariable;
}
