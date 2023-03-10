using Hyper.Compiler.Symbol;

namespace Hyper.Compiler.Binding;

internal sealed class BoundVariableExpression : BoundExpression
{
    public BoundVariableExpression(VariableSymbol variable)
    {
        Variable = variable;
    }

    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    public          string        Name { get; }

    public override Type           Type     => Variable.Type;
    public          VariableSymbol Variable { get; }
}
