using Hyper.Core.Symbols;

namespace Hyper.Core.Binding.Expr;

internal sealed class BoundErrorExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
    public override TypeSymbol    Type => TypeSymbol.Error;
}
