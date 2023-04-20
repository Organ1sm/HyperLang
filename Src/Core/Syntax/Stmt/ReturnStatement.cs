using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class ReturnStatement : Statement
{
    public ReturnStatement(Token returnKeyword, Expression? expression)
    {
        ReturnKeyword = returnKeyword;
        Expression = expression;
    }

    public override SyntaxKind  Kind          => SyntaxKind.ReturnStatement;
    public          Token       ReturnKeyword { get; }
    public          Expression? Expression    { get; }
}
