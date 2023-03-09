namespace Hyper.Compiler.Binding;

internal abstract  class BoundExpression : BoundNode
{
    public abstract Type Type { get; }
}
