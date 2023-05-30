using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed partial class ExpressionStatement : Statement
{
    public ExpressionStatement(AST syntaxTree, Expression expression)
        : base(syntaxTree)
    {
        Expression = expression;
    }

    public override SyntaxKind Kind       => SyntaxKind.ExpressionStatement;
    public          Expression Expression { get; }
}
