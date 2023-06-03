using Hyper.Core.Binder.Expr;

namespace Hyper.Core.Symbols;

public abstract class VariableSymbol : Symbol
{
    internal VariableSymbol(string name, TypeSymbol type, bool isReadOnly, BoundConstant? constant) : base(name)
    {
        Type = type;
        IsReadOnly = isReadOnly;
        Constant = isReadOnly ? constant : null;
    }

    public   TypeSymbol Type       { get; }
    public   bool       IsReadOnly { get; }
    internal BoundConstant? Constant   { get; }
}
