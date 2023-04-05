using Hyper.Core.Binding.Expr;

namespace Hyper.Core.Binding.Stmt;

internal sealed class BoundGotoStatement : BoundStatement
{
    public BoundGotoStatement(BoundLabel label)
    {
        Label = label;
    }

    public override BoundNodeKind Kind  => BoundNodeKind.GotoStatement;
    public          BoundLabel    Label { get; }
}
