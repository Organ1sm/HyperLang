using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax.Expr;

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
