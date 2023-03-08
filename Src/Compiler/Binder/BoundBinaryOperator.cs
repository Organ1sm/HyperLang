using Hyper.Compiler.Syntax;

namespace Hyper.Compiler.Binding
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxKind kind, BoundBinaryOperatorKind opKind, Type type)
            : this(kind, opKind, type, type, type) { }

        private BoundBinaryOperator(SyntaxKind kind,
                                    BoundBinaryOperatorKind opKind,
                                    Type leftType,
                                    Type rightTye,
                                    Type resultType)
        {
            Kind = kind;
            OPKind = opKind;
            LeftType = leftType;
            RightTye = rightTye;
            ResultType = resultType;
        }

        public SyntaxKind              Kind       { get; }
        public BoundBinaryOperatorKind OPKind     { get; }
        public Type                    LeftType   { get; }
        public Type                    RightTye   { get; }
        public Type                    ResultType { get; }

        private static BoundBinaryOperator[] _operators =
        {
            new BoundBinaryOperator(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.AmpersandAmpersandToken,
                                    BoundBinaryOperatorKind.LogicalAnd,
                                    typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, typeof(bool)),
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
