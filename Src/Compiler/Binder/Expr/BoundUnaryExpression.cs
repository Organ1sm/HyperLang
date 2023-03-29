using Hyper.Compiler.Symbols;

namespace Hyper.Compiler.Binding
{
    internal class BoundUnaryExpression : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override TypeSymbol    Type => Operator.Type;

        public BoundUnaryOperator Operator { get; }
        public BoundExpression    Operand  { get; }

        public BoundUnaryExpression(BoundUnaryOperator @operator, BoundExpression operand)
        {
            Operator = @operator;
            Operand = operand;
        }
    }
}
