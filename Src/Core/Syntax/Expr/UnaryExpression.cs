using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed class UnaryExpression : Expression
{
    public override SyntaxKind Kind     => SyntaxKind.UnaryExpression;
    public override IEnumerable<Node> GetChildren()
    {
        yield return Operator;
        yield return Operand;
    }

    public          Token      Operator { get; }
    public          Expression Operand  { get; }

    public UnaryExpression(AST syntaxTree, Token @operator, Expression operand)
        : base(syntaxTree)
    {
        Operator = @operator;
        Operand = operand;
    }
}
