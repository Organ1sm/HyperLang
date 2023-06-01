using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class ExpressionStatement : Statement
{
    public ExpressionStatement(AST syntaxTree, Expression expression)
        : base(syntaxTree)
    {
        Expression = expression;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Expression;
    }

    public override SyntaxKind Kind       => SyntaxKind.ExpressionStatement;
    public          Expression Expression { get; }
}
