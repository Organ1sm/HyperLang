using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class WhileStatement : Statement
{
    public WhileStatement(AST syntaxTree, Token whileKeyword, Expression condition, Statement body)
        : base(syntaxTree)
    {
        WhileKeyword = whileKeyword;
        Condition = condition;
        Body = body;
    }

    public override SyntaxKind Kind => SyntaxKind.WhileStatement;

    public override IEnumerable<Node> GetChildren()
    {
        yield return WhileKeyword;
        yield return Condition;
        yield return Body;
    }

    public Token      WhileKeyword { get; }
    public Expression Condition    { get; }
    public Statement  Body         { get; }
}
