using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class ReturnStatement : Statement
{
    public ReturnStatement(AST syntaxTree, Token returnKeyword, Expression? expression)
        : base(syntaxTree)
    {
        ReturnKeyword = returnKeyword;
        Expression = expression;
    }

    public override SyntaxKind Kind => SyntaxKind.ReturnStatement;

    public override IEnumerable<Node> GetChildren()
    {
        yield return ReturnKeyword;

        if (Expression != null)
            yield return Expression;
    }

    public Token       ReturnKeyword { get; }
    public Expression? Expression    { get; }
}
