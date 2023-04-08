using Hyper.Core.Binding.Expr;

namespace Hyper.Core.Binding.Stmt;

internal sealed class BoundLabelStatement : BoundStatement
{
    public BoundLabelStatement(BoundLabel label)
    {
        Label = label;
    }

    public override BoundNodeKind Kind  => BoundNodeKind.LabelStatement;
    public          BoundLabel    Label { get; }
}
