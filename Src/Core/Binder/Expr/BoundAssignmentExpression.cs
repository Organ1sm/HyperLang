using Hyper.Core.Symbols;

namespace Hyper.Core.Binding.Expr;

internal class BoundAssignmentExpression : BoundExpression
{
    public BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression)
    {
        Expression = expression;
        Variable = variable;
    }

    public override BoundNodeKind   Kind       => BoundNodeKind.AssignmentExpression;
    public override TypeSymbol      Type       => Expression.Type;
    public          VariableSymbol  Variable   { get; }
    public          BoundExpression Expression { get; }
}
