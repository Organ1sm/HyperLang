using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax.Stmt;

public class IfStatement : Statement
{
    public IfStatement(Token ifKeyword, Expression condition, Statement thenStatement, ElseClause? elseClause)
    {
        IfKeyword = ifKeyword;
        Condition = condition;
        ThenStatement = thenStatement;
        ElseClause = elseClause;
    }

    public override SyntaxKind Kind          => SyntaxKind.IfStatement;
    public          Token      IfKeyword     { get; }
    public          Expression Condition     { get; }
    public          Statement  ThenStatement { get; }
    public          ElseClause? ElseClause    { get; }
}
