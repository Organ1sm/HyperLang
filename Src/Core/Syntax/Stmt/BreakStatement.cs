using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Stmt;

internal class BreakStatement : Statement
{
    public BreakStatement(Token keyword)
    {
        Keyword = keyword;
    }

    public override SyntaxKind Kind    => SyntaxKind.BreakStatement;
    public          Token      Keyword { get; }
}
