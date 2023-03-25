using Hyper.Compiler.Symbols;
using Hyper.Compiler.Syntax;

namespace Hyper.Compiler.Binding
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxKind kind, BoundBinaryOperatorKind opKind, TypeSymbol type)
            : this(kind, opKind, type, type, type) { }

        private BoundBinaryOperator(SyntaxKind syntaxKind,
                                    BoundBinaryOperatorKind kind,
                                    TypeSymbol operandType,
                                    TypeSymbol resultType)
            : this(syntaxKind, kind, operandType, operandType, resultType) { }

        private BoundBinaryOperator(SyntaxKind kind,
                                    BoundBinaryOperatorKind opKind,
                                    TypeSymbol leftType,
                                    TypeSymbol rightTye,
                                    TypeSymbol resultType)
        {
            Kind = kind;
            OpKind = opKind;
            LeftType = leftType;
            RightTye = rightTye;
            Type = resultType;
        }

        public SyntaxKind              Kind     { get; }
        public BoundBinaryOperatorKind OpKind   { get; }
        public TypeSymbol              LeftType { get; }
        public TypeSymbol              RightTye { get; }
        public TypeSymbol              Type     { get; }

        private static BoundBinaryOperator[] _operators =
        {
            new BoundBinaryOperator(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Int),

            new BoundBinaryOperator(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Int),

            new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken,
                                    BoundBinaryOperatorKind.Equals,
                                    TypeSymbol.Int,
                                    TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.BangEqualsToken,
                                    BoundBinaryOperatorKind.NotEquals,
                                    TypeSymbol.Int,
                                    TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.LessToken,
                                    BoundBinaryOperatorKind.Less,
                                    TypeSymbol.Int,
                                    TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.LessOrEqualsToken,
                                    BoundBinaryOperatorKind.LessOrEquals,
                                    TypeSymbol.Int,
                                    TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.GreaterToken,
                                    BoundBinaryOperatorKind.Greater,
                                    TypeSymbol.Int,
                                    TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.GreaterOrEqualsToken,
                                    BoundBinaryOperatorKind.GreaterOrEquals,
                                    TypeSymbol.Int,
                                    TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.AmpersandAmpersandToken,
                                    BoundBinaryOperatorKind.LogicalAnd,
                                    TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Bool),
        };

        public static BoundBinaryOperator Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
        {
            foreach (var op in _operators)
            {
                if (op.Kind == syntaxKind && op.LeftType == leftType && op.RightTye == rightType)
                    return op;
            }

            return null;
        }
    }
}
