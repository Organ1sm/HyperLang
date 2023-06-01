using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

internal sealed class ContinueStatement : Statement
{
    public ContinueStatement(AST syntaxTree, Token keyword)
        : base(syntaxTree)
    {
        Keyword = keyword;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Keyword;
    }

    public override SyntaxKind Kind    => SyntaxKind.ContinueStatement;
    public          Token      Keyword { get; }
}
