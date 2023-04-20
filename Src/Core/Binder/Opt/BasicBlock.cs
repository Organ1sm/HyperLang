using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Stmt;

namespace Hyper.Core.Binding.Opt;

internal sealed class BasicBlock
{
    public BasicBlock() { }

    public BasicBlock(bool isStart)
    {
        IsStart = isStart;
        IsEnd = !isStart;
    }

    public override string ToString()
    {
        if (IsStart) return "<Start>";
        if (IsEnd) return "<End>";

        using (var writer = new StringWriter())
        {
            foreach (var statement in Statements)
                statement.WriteTo(writer);

            return writer.ToString();
        }
    }

    // Indicates whether this block is the start block of a function.
    public bool IsStart { get; }

    // Indicates whether this block is the end block of a function.
    public bool IsEnd { get; }

    // The statements (instructions) in this basic block.
    public List<BoundStatement> Statements { get; } = new();

    // The incoming branches to this basic block. 
    public List<BasicBlockBranch> Incoming { get; } = new();

    /// The outgoing branches from this basic block.
    public List<BasicBlockBranch> Outgoing { get; } = new();
}
