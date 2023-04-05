using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class WhileStatement : Statement
{
    public WhileStatement(Token whileKeyword, Expression condition, Statement body)
    {
        WhileKeyword = whileKeyword;
        Condition = condition;
        Body = body;
    }

    public override SyntaxKind Kind         => SyntaxKind.WhileStatement;
    public          Token      WhileKeyword { get; }
    public          Expression Condition    { get; }
    public          Statement  Body         { get; }
}
