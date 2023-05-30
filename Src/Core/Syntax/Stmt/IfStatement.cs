using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed partial class IfStatement : Statement
{
    public IfStatement(AST syntaxTree,
                       Token ifKeyword,
                       Expression condition,
                       Statement thenStatement,
                       ElseClause? elseClause)
        : base(syntaxTree)
    {
        IfKeyword = ifKeyword;
        Condition = condition;
        ThenStatement = thenStatement;
        ElseClause = elseClause;
    }

    public override SyntaxKind  Kind          => SyntaxKind.IfStatement;
    public          Token       IfKeyword     { get; }
    public          Expression  Condition     { get; }
    public          Statement   ThenStatement { get; }
    public          ElseClause? ElseClause    { get; }
}