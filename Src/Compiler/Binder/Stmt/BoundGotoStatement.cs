using Hyper.Compiler.Symbol;

namespace Hyper.Compiler.Binding;

internal sealed class BoundGotoStatement : BoundStatement
{
    public BoundGotoStatement(LabelSymbol label)
    {
        Label = label;
    }

    public override BoundNodeKind Kind  => BoundNodeKind.GotoStatement;
    public          LabelSymbol   Label { get; }
}
