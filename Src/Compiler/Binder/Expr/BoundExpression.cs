using Hyper.Compiler.Symbols;

namespace Hyper.Compiler.Binding;

internal abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type { get; }
}
