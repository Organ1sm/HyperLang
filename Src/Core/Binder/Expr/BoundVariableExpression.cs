using Hyper.Core.Symbols;

namespace Hyper.Core.Binding.Expr;

internal sealed class BoundVariableExpression : BoundExpression
{
    public BoundVariableExpression(VariableSymbol variable)
    {
        Variable = variable;
    }

    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;

    public override TypeSymbol      Type     => Variable?.Type ?? TypeSymbol.Error;
    public          VariableSymbol Variable { get; }
}
