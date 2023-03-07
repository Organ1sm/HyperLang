namespace Hyper.Compiler.Binding
{
    internal class BoundUnaryExpression : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override Type          Type => Operand.Type;

        public BoundUnaryOperatorKind OperatorKind { get; }
        public BoundExpression        Operand      { get; }

        public BoundUnaryExpression(BoundUnaryOperatorKind operatorKind, BoundExpression operand)
        {
            OperatorKind = operatorKind;
            Operand = operand;
        }
    }
}
