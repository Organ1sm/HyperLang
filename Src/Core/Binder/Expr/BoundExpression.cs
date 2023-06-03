using Hyper.Core.Binder.Expr;
using Hyper.Core.Symbols;

namespace Hyper.Core.Binding.Expr;

internal abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol     Type          { get; }
    public virtual  BoundConstant? ConstantValue => null;
}
