using Hyper.Compiler.Symbols;

namespace Hyper.Compiler.Binding
{
    internal class BoundLiteralExpression : BoundExpression
    {
        public override BoundNodeKind Kind  => BoundNodeKind.LiteralExpression;
        public override TypeSymbol    Type  { get; }
        public          object        Value { get; }

        public BoundLiteralExpression(object value)
        {
            Value = value;

            Type = value switch
            {
                bool   => TypeSymbol.Bool,
                int    => TypeSymbol.Int,
                string => TypeSymbol.String,
                _      => throw new Exception($"Unexpected literal '{value}' of type {value.GetType()}")
            };
        }
    }
}
