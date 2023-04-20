using Hyper.Core.Binding.Expr;

namespace Hyper.Core.Binding.Opt;

internal sealed class BasicBlockBranch
{
    public BasicBlockBranch(BasicBlock from, BasicBlock to, BoundExpression? condition)
    {
        From = from;
        To = to;
        Condition = condition;
    }


    public override string ToString() => Condition == null ? string.Empty : Condition.ToString();

    // Gets the branch condition (null if unconditional).
    public BasicBlock From { get; }

    // Gets the source basic block.
    public BasicBlock To { get; }

    // Gets the destination basic block.
    public BoundExpression? Condition { get; }
}
