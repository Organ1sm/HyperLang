using Hyper.Core.Symbols;

namespace Hyper.Core.Binding.Expr;

internal sealed class BoundConversionExpression : BoundExpression
{
    public BoundConversionExpression(TypeSymbol type, BoundExpression expression)
    {
        Type = type;
        Expression = expression;
    }

    public override BoundNodeKind   Kind       => BoundNodeKind.ConversionExpression;
    public override TypeSymbol      Type       { get; }
    public          BoundExpression Expression { get; }
}
