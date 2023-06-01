using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class IfStatement : Statement
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

    public override SyntaxKind Kind => SyntaxKind.IfStatement;

    public override IEnumerable<Node> GetChildren()
    {
        yield return IfKeyword;
        yield return Condition;
        yield return ThenStatement;

        if (ElseClause != null)
            yield return ElseClause;
    }

    public Token       IfKeyword     { get; }
    public Expression  Condition     { get; }
    public Statement   ThenStatement { get; }
    public ElseClause? ElseClause    { get; }
}
