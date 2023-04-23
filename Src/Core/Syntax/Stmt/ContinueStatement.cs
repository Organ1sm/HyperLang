using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Stmt;

internal class ContinueStatement : Statement
{
    public ContinueStatement(AST syntaxTree, Token keyword)
        : base(syntaxTree)
    {
        Keyword = keyword;
    }

    public override SyntaxKind Kind    => SyntaxKind.ContinueStatement;
    public          Token      Keyword { get; }
}
