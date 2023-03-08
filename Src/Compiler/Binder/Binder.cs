using System.Diagnostics.SymbolStore;
using Hyper.Compiler.Syntax;

namespace Hyper.Compiler.Binding
{
    internal sealed class Binder
    {
        private readonly List<string>        _diagnostics = new();
        public           IEnumerable<string> Diagnostics => _diagnostics;

        public BoundExpression BindExpression(Expression syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.LiteralExpression => BindLiteralExpression((LiteralExpression) syntax),
                SyntaxKind.UnaryExpression   => BindUnaryExpression((UnaryExpression) syntax),
                SyntaxKind.BinaryExpression  => BindBinaryExpression((BinaryExpression) syntax),
                _                            => throw new ArgumentException($"Unexpected syntax {syntax.Kind}")
            };
        }

        private BoundLiteralExpression BindLiteralExpression(LiteralExpression syntax)
        {
            var value = syntax.Value ?? 0;

            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpression syntax)
        {
            var boundOperand      = BindExpression(syntax.Operand);
            var boundOperatorKind = BindUnaryOperatorKind(syntax.OperatorToken.Kind, boundOperand.Type);

            if (boundOperatorKind == null)
            {
                _diagnostics
                    .Add($"Unary operator '{syntax.OperatorToken.Text}' is not defined for type {boundOperand.Type}");
                return boundOperand;
            }

            return new BoundUnaryExpression(boundOperatorKind.Value, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpression syntax)
        {
            var boundLeft         = BindExpression(syntax.Left);
            var boundRight        = BindExpression(syntax.Right);
            var boundOperatorKind = BindBinaryOperatorKind(syntax.Operator.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperatorKind == null)
            {
                _diagnostics
                    .Add($"Binary operator '{syntax.Operator.Text}' is not defined for types {boundLeft.Type} and {boundRight.Type}.");
                return boundLeft;
            }

            return new BoundBinaryExpression(boundLeft, boundOperatorKind.Value, boundRight);
        }

        private BoundUnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind kind, Type operandType)
        {
            if (operandType == typeof(int))
            {
                return kind switch
                {
                    SyntaxKind.PlusToken  => BoundUnaryOperatorKind.Identity,
                    SyntaxKind.MinusToken => BoundUnaryOperatorKind.Negation
                };
            }

            if (operandType == typeof(bool))
            {
                return kind switch
                {
                    SyntaxKind.BangToken => BoundUnaryOperatorKind.LogicalNegation
                };
            }

            return null;
        }

        private BoundBinaryOperatorKind? BindBinaryOperatorKind(SyntaxKind kind, Type leftType, Type rightType)
        {
            if (leftType == typeof(int) && rightType == typeof(int))
            {
                return kind switch
                {
                    SyntaxKind.PlusToken  => BoundBinaryOperatorKind.Addition,
                    SyntaxKind.MinusToken => BoundBinaryOperatorKind.Subtraction,
                    SyntaxKind.StarToken  => BoundBinaryOperatorKind.Multiplication,
                    SyntaxKind.SlashToken => BoundBinaryOperatorKind.Division,
                    _                     => null
                };
            }

            if (leftType == typeof(bool) && rightType == typeof(bool))
            {
                return kind switch
                {
                    SyntaxKind.AmpersandAmpersandToken => BoundBinaryOperatorKind.LogicalAnd,
                    SyntaxKind.PipePipeToken           => BoundBinaryOperatorKind.LogicalOr,
                    _                                  => null
                };
            }

            return null;
        }
    }
}
