using Hyper.Core.Binder.Expr;
using Hyper.Core.Binding.Operator;
using Hyper.Core.Binding.Opt;
using Hyper.Core.Symbols;

namespace Hyper.Core.Binding.Expr
{
    internal class BoundUnaryExpression : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override TypeSymbol    Type => Operator.Type;

        public          BoundUnaryOperator Operator      { get; }
        public          BoundExpression    Operand       { get; }
        public override BoundConstant?     ConstantValue { get; }

        public BoundUnaryExpression(BoundUnaryOperator @operator, BoundExpression operand)
        {
            Operator = @operator;
            Operand = operand;
            ConstantValue = ConstantFolding.ComputeConstant(@operator, operand);
        }
    }
}
