using Hyper.Core.Syntax.Expr;
using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Stmt;

public sealed class ElseClause : Node
{
    public ElseClause(AST syntaxTree, Token elseKeyword, Statement elseStatement)
        : base(syntaxTree)
    {
        ElseKeyword = elseKeyword;
        ElseStatement = elseStatement;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return ElseKeyword;
        yield return ElseStatement;
    }

    public override SyntaxKind Kind          => SyntaxKind.ElseClause;
    public          Token      ElseKeyword   { get; }
    public          Statement  ElseStatement { get; }
}
