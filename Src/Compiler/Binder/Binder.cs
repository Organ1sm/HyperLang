using System.Diagnostics.SymbolStore;
using Hyper.Compiler.Diagnostic;
using Hyper.Compiler.Syntax;

namespace Hyper.Compiler.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag                      _diagnostics = new();
        public           IEnumerable<Diagnostic.Diagnostic> Diagnostics => _diagnostics;

        public BoundExpression BindExpression(Expression syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.LiteralExpression       => BindLiteralExpression((LiteralExpression) syntax),
                SyntaxKind.UnaryExpression         => BindUnaryExpression((UnaryExpression) syntax),
                SyntaxKind.BinaryExpression        => BindBinaryExpression((BinaryExpression) syntax),
                SyntaxKind.ParenthesizedExpression => BindExpression(((ParenthesizedExpression) syntax).Expression),
                _                                  => throw new ArgumentException($"Unexpected syntax {syntax.Kind}")
            };
        }

        private BoundLiteralExpression BindLiteralExpression(LiteralExpression syntax)
        {
            var value = syntax.Value ?? 0;

            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpression syntax)
        {
            var boundOperand  = BindExpression(syntax.Operand);
            var boundOperator = BoundUnaryOperator.Bind(syntax.Operator.Kind, boundOperand.Type);

            if (boundOperator == null)
            {
                _diagnostics
                    .ReportUndefinedUnaryOperator(syntax.Operator.Span,
                                                  syntax.Operator.Text,
                                                  boundOperand.Type);
                return boundOperand;
            }

            return new BoundUnaryExpression(boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpression syntax)
        {
            var boundLeft     = BindExpression(syntax.Left);
            var boundRight    = BindExpression(syntax.Right);
            var boundOperator = BoundBinaryOperator.Bind(syntax.Operator.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperator == null)
            {
                _diagnostics
                    .ReportUndefinedBinaryOperator(syntax.Operator.Span,
                                                   syntax.Operator.Text,
                                                   boundLeft.Type,
                                                   boundRight.Type);
                return boundLeft;
            }

            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }
    }
}
