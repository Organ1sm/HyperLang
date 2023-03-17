namespace Hyper.Compiler.Syntax.Stmt;

public sealed class ExpressionStatement : Statement
{
    public ExpressionStatement(Expression expression)
    {
        Expression = expression;
    }

    public override SyntaxKind Kind       => SyntaxKind.ExpressionStatement;
    public          Expression Expression { get; }
}
