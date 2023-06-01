using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class GlobalStatement : MemberSyntax
{
    public GlobalStatement(AST syntaxTree, Statement statement)
        : base(syntaxTree)
    {
        Statement = statement;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Statement;
    }

    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;

    public Statement Statement { get; }
}
