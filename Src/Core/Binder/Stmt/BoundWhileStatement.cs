using Hyper.Core.Binding.Expr;

namespace Hyper.Core.Binding.Stmt;

internal sealed class BoundWhileStatement : BoundStatement
{
    public BoundWhileStatement(BoundExpression condition, BoundStatement? body)
    {
        Condition = condition;
        Body = body;
    }

    public override BoundNodeKind   Kind      => BoundNodeKind.WhileStatement;
    public          BoundExpression Condition { get; }
    public          BoundStatement? Body      { get; }
}
