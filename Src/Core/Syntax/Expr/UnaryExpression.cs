using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed class UnaryExpression : Expression
{
    public override SyntaxKind Kind     => SyntaxKind.UnaryExpression;
    public          Token      Operator { get; }
    public          Expression Operand  { get; }

    public UnaryExpression(Token @operator, Expression operand)
    {
        Operator = @operator;
        Operand = operand;
    }
}
