using Hyper.Compiler.Symbol;

namespace Hyper.Compiler.Binding;

internal sealed class BoundLabelStatement : BoundStatement
{
    public BoundLabelStatement(LabelSymbol label)
    {
        Label = label;
    }

    public override BoundNodeKind Kind  => BoundNodeKind.LabelStatement;
    public          LabelSymbol   Label { get; }
}
