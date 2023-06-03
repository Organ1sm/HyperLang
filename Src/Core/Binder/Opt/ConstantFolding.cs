using Hyper.Core.Binder.Expr;
using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Operator;
using Hyper.Core.Symbols;

namespace Hyper.Core.Binding.Opt;

internal static class ConstantFolding
{
    public static BoundConstant? ComputeConstant(BoundUnaryOperator op, BoundExpression operand)
    {
        if (operand.ConstantValue != null)
        {
            return op.OpKind switch
            {
                BoundUnaryOperatorKind.Identity        => new BoundConstant((int) operand.ConstantValue.Value),
                BoundUnaryOperatorKind.Negation        => new BoundConstant(-(int) operand.ConstantValue.Value),
                BoundUnaryOperatorKind.LogicalNegation => new BoundConstant(!(bool) operand.ConstantValue.Value),
                BoundUnaryOperatorKind.OnesComplement  => new BoundConstant(~(int) operand.ConstantValue.Value),
                _                                      => throw new Exception($"Unexpected unary operator {op.OpKind}")
            };
        }

        return null;
    }

    public static BoundConstant? ComputeConstant(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
    {
        var leftConstant  = left.ConstantValue;
        var rightConstant = right.ConstantValue;

        // Special case && and || because there are cases where only one
        // side needs to be known.
        if (op.OpKind == BoundBinaryOperatorKind.LogicalAnd)
        {
            if (leftConstant != null && !(bool) leftConstant.Value ||
                rightConstant != null && !(bool) rightConstant.Value)
            {
                return new BoundConstant(false);
            }
        }

        if (op.OpKind == BoundBinaryOperatorKind.LogicalOr)
        {
            if (leftConstant != null && (bool) leftConstant.Value ||
                rightConstant != null && (bool) rightConstant.Value)
            {
                return new BoundConstant(true);
            }
        }

        if (leftConstant == null || rightConstant == null)
            return null;

        var l = leftConstant.Value;
        var r = rightConstant.Value;

        return op.OpKind switch
        {
            BoundBinaryOperatorKind.Addition => left.Type == TypeSymbol.Int
                ? new BoundConstant((int) l + (int) r)
                : new BoundConstant((string) l + (string) r),
            BoundBinaryOperatorKind.Subtraction    => new BoundConstant((int) l - (int) r),
            BoundBinaryOperatorKind.Multiplication => new BoundConstant((int) l * (int) r),
            BoundBinaryOperatorKind.Division       => new BoundConstant((int) l / (int) r),
            BoundBinaryOperatorKind.BitwiseAnd => left.Type == TypeSymbol.Int
                ? new BoundConstant((int) l & (int) r)
                : new BoundConstant((bool) l & (bool) r),
            BoundBinaryOperatorKind.BitwiseOr => left.Type == TypeSymbol.Int
                ? new BoundConstant((int) l | (int) r)
                : new BoundConstant((bool) l | (bool) r),
            BoundBinaryOperatorKind.BitwiseXor => left.Type == TypeSymbol.Int
                ? new BoundConstant((int) l ^ (int) r)
                : new BoundConstant((bool) l ^ (bool) r),
            BoundBinaryOperatorKind.LogicalAnd      => new BoundConstant((bool) l && (bool) r),
            BoundBinaryOperatorKind.LogicalOr       => new BoundConstant((bool) l || (bool) r),
            BoundBinaryOperatorKind.Equals          => new BoundConstant(Equals(l, r)),
            BoundBinaryOperatorKind.NotEquals       => new BoundConstant(!Equals(l, r)),
            BoundBinaryOperatorKind.Less            => new BoundConstant((int) l < (int) r),
            BoundBinaryOperatorKind.LessOrEquals    => new BoundConstant((int) l <= (int) r),
            BoundBinaryOperatorKind.Greater         => new BoundConstant((int) l > (int) r),
            BoundBinaryOperatorKind.GreaterOrEquals => new BoundConstant((int) l >= (int) r),
            _                                       => throw new Exception($"Unexpected binary operator {op.OpKind}")
        };
    }
}
