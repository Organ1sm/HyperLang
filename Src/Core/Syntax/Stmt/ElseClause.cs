using Hyper.Core.Syntax.Expr;
using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Stmt;

public sealed partial class ElseClause : Node
{
    public ElseClause(AST syntaxTree, Token elseKeyword, Statement elseStatement)
        : base(syntaxTree)
    {
        ElseKeyword = elseKeyword;
        ElseStatement = elseStatement;
    }

    public override SyntaxKind Kind          => SyntaxKind.ElseClause;
    public          Token      ElseKeyword   { get; }
    public          Statement  ElseStatement { get; }
}
