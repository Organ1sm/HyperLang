using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

internal sealed class BreakStatement : Statement
{
    public BreakStatement(AST syntaxTree, Token keyword)
        : base(syntaxTree)
    {
        Keyword = keyword;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Keyword;
    }

    public override SyntaxKind Kind    => SyntaxKind.BreakStatement;
    public          Token      Keyword { get; }
}
