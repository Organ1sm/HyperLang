namespace Hyper.Core.Syntax.Stmt;

public sealed partial class GlobalStatement : MemberSyntax
{
    public GlobalStatement(AST syntaxTree, Statement statement)
        : base(syntaxTree)
    {
        Statement = statement;
    }

    public override SyntaxKind Kind      => SyntaxKind.GlobalStatement;
    public          Statement  Statement { get; }
}
