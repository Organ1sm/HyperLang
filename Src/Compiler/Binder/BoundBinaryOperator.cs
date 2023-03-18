using Hyper.Compiler.Syntax;

namespace Hyper.Compiler.Binding
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxKind kind, BoundBinaryOperatorKind opKind, Type type)
            : this(kind, opKind, type, type, type) { }

        private BoundBinaryOperator(SyntaxKind syntaxKind,
                                    BoundBinaryOperatorKind kind,
                                    Type operandType,
                                    Type resultType)
            : this(syntaxKind, kind, operandType, operandType, resultType) { }

        private BoundBinaryOperator(SyntaxKind kind,
                                    BoundBinaryOperatorKind opKind,
                                    Type leftType,
                                    Type rightTye,
                                    Type resultType)
        {
            Kind = kind;
            OpKind = opKind;
            LeftType = leftType;
            RightTye = rightTye;
            Type = resultType;
        }

        public SyntaxKind              Kind     { get; }
        public BoundBinaryOperatorKind OpKind   { get; }
        public Type                    LeftType { get; }
        public Type                    RightTye { get; }
        public Type                    Type     { get; }

        private static BoundBinaryOperator[] _operators =
        {
            new BoundBinaryOperator(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken,
                                    BoundBinaryOperatorKind.Equals,
                                    typeof(int),
                                    typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.BangEqualsToken,
                                    BoundBinaryOperatorKind.NotEquals,
                                    typeof(int),
                                    typeof(bool)),

            new BoundBinaryOperator(SyntaxKind.LessToken, BoundBinaryOperatorKind.Less, typeof(int), typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.LessOrEqualsToken,
                                    BoundBinaryOperatorKind.LessOrEquals,
                                    typeof(int),
                                    typeof(bool)),
            
            new BoundBinaryOperator(SyntaxKind.GreaterToken,
                                    BoundBinaryOperatorKind.Greater,
                                    typeof(int),
                                    typeof(bool)),
            
            new BoundBinaryOperator(SyntaxKind.GreaterOrEqualsToken,
                                    BoundBinaryOperatorKind.GreaterOrEquals,
                                    typeof(int),
                                    typeof(bool)),

            new BoundBinaryOperator(SyntaxKind.AmpersandAmpersandToken,
                                    BoundBinaryOperatorKind.LogicalAnd,
                                    typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, typeof(bool)),
        };

        public static BoundBinaryOperator Bind(SyntaxKind syntaxKind, Type leftType, Type rightType)
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
