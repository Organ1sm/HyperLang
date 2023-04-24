using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Stmt;

internal class BreakStatement : Statement
{
    public BreakStatement(AST syntaxTree, Token keyword)
        : base(syntaxTree)
    {
        Keyword = keyword;
    }

    public override SyntaxKind Kind    => SyntaxKind.BreakStatement;
    public          Token      Keyword { get; }
}
