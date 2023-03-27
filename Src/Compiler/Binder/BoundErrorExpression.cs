using Hyper.Compiler.Symbols;

namespace Hyper.Compiler.Binding;

internal sealed class BoundErrorExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
    public override TypeSymbol    Type => TypeSymbol.Error;
}
