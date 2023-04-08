using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Stmt;

internal class ContinueStatement : Statement
{
    public ContinueStatement(Token keyword)
    {
        Keyword = keyword;
    }

    public override SyntaxKind Kind    => SyntaxKind.ContinueStatement;
    public          Token      Keyword { get; }
}
