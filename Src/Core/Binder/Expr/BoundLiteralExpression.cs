using Hyper.Core.Binder.Expr;
using Hyper.Core.Symbols;

namespace Hyper.Core.Binding.Expr
{
    internal class BoundLiteralExpression : BoundExpression
    {
        public override BoundNodeKind  Kind          => BoundNodeKind.LiteralExpression;
        public override TypeSymbol     Type          { get; }
        public          object        Value         => ConstantValue.Value;
        public override BoundConstant? ConstantValue { get; }

        public BoundLiteralExpression(object value)
        {
            Type = value switch
            {
                bool   => TypeSymbol.Bool,
                int    => TypeSymbol.Int,
                string => TypeSymbol.String,
                _      => throw new Exception($"Unexpected literal '{value}' of type {value?.GetType()}")
            };

            ConstantValue = new BoundConstant(value);
        }
    }
}
