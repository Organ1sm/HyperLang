namespace Hyper.Compiler.Binding
{
    internal class BoundBinaryExpression : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override Type          Type => Left.Type;

        public BoundExpression         Left         { get; }
        public BoundBinaryOperator Operator { get; }
        public BoundExpression         Right        { get; }

        public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator @operator, BoundExpression right)
        {
            Left = left;
            Operator = @operator;
            Right = right;
        }
    }
}
