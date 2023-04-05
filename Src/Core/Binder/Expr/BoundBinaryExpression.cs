using Hyper.Core.Binding.Operator;
using Hyper.Core.Symbols;

namespace Hyper.Core.Binding.Expr
{
    internal class BoundBinaryExpression : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
        public override TypeSymbol    Type => Operator.Type;

        public BoundExpression      Left     { get; }
        public BoundBinaryOperator? Operator { get; }
        public BoundExpression      Right    { get; }

        public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator? @operator, BoundExpression right)
        {
            Left = left;
            Operator = @operator;
            Right = right;
        }
    }
}
