namespace Hyper.Core.Binder.Expr;

internal class BoundConstant
{
    public BoundConstant(object value) => Value = value;
    public object Value { get; }
}
