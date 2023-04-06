using Hyper.Core.Symbols;
using Hyper.Core.Syntax;

namespace Hyper.Core.Binding.Operator
{
    internal sealed class BoundUnaryOperator
    {
        private BoundUnaryOperator(SyntaxKind kind, BoundUnaryOperatorKind opKind, TypeSymbol operandType)
            : this(kind, opKind, operandType, operandType) { }

        public BoundUnaryOperator(SyntaxKind kind,
                                  BoundUnaryOperatorKind opKind,
                                  TypeSymbol operandType,
                                  TypeSymbol resultType)
        {
            Kind = kind;
            OpKind = opKind;
            OperandType = operandType;
            Type = resultType;
        }

        public SyntaxKind             Kind        { get; }
        public BoundUnaryOperatorKind OpKind      { get; }
        public TypeSymbol             OperandType { get; }
        public TypeSymbol             Type        { get; }

        private static BoundUnaryOperator[] _operators =
        {
            new BoundUnaryOperator(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, TypeSymbol.Bool),
            new BoundUnaryOperator(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, TypeSymbol.Int),
            new BoundUnaryOperator(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, TypeSymbol.Int),
            new BoundUnaryOperator(SyntaxKind.TildeToken, BoundUnaryOperatorKind.OnesComplement, TypeSymbol.Int),
        };

        public static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol operandType)
        {
            foreach (var op in _operators)
            {
                if (op.Kind == syntaxKind && op.OperandType == operandType)
                    return op;
            }

            return null;
        }
    }
}
