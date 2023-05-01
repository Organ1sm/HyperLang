using Hyper.Core.Binding.Expr;

namespace Hyper.Core.Binding.Stmt;

internal sealed class BoundWhileStatement : BoundLoopStatement
{
    public BoundWhileStatement(BoundExpression condition,
                               BoundStatement body,
                               BoundLabel breakLabel,
                               BoundLabel continueLabel)
        : base(breakLabel, continueLabel)
    {
        Condition = condition;
        Body = body;
    }

    public override BoundNodeKind   Kind      => BoundNodeKind.WhileStatement;
    public          BoundExpression Condition { get; }
    public          BoundStatement  Body      { get; }
}
