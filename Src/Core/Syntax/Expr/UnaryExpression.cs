using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed partial class UnaryExpression : Expression
{
    public override SyntaxKind Kind     => SyntaxKind.UnaryExpression;
    public          Token      Operator { get; }
    public          Expression Operand  { get; }

    public UnaryExpression(AST syntaxTree, Token @operator, Expression operand)
        : base(syntaxTree)
    {
        Operator = @operator;
        Operand = operand;
    }
}
