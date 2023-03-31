namespace Hyper.Compiler.Syntax.Stmt;

public sealed class GlobalStatement : MemberSyntax
{
    public GlobalStatement(Statement statement)
    {
        Statement = statement;
    }

    public override SyntaxKind Kind      => SyntaxKind.GlobalStatement;
    public          Statement  Statement { get; }
}
