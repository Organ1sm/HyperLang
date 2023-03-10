using System.Diagnostics.SymbolStore;
using Hyper.Compiler.Diagnostic;
using Hyper.Compiler.Symbol;
using Hyper.Compiler.Syntax;

namespace Hyper.Compiler.Binding
{
    internal sealed class Binder
    {
        private readonly Dictionary<VariableSymbol, object> _variables;
        private readonly DiagnosticBag                      _diagnostics = new();
        public           IEnumerable<Diagnostic.Diagnostic> Diagnostics => _diagnostics;

        public Binder(Dictionary<VariableSymbol, object> variables)
        {
            _variables = variables;
        }

        public BoundExpression BindExpression(Expression syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.LiteralExpression       => BindLiteralExpression((LiteralExpression) syntax),
                SyntaxKind.UnaryExpression         => BindUnaryExpression((UnaryExpression) syntax),
                SyntaxKind.BinaryExpression        => BindBinaryExpression((BinaryExpression) syntax),
                SyntaxKind.ParenthesizedExpression => BindParenthesizedExpression((ParenthesizedExpression) syntax),
                SyntaxKind.NameExpression          => BindNameExpression((NameExpression) syntax),
                SyntaxKind.AssignmentExpression    => BindAssignmentExpression((AssignmentExpression) syntax),
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

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpression syntax)
        {
            return BindExpression(syntax.Expression);
        }

        private BoundExpression BindNameExpression(NameExpression syntax)
        {
            var name     = syntax.IdentifierToken.Text;
            var variable = _variables.Keys.FirstOrDefault(v => v.Name == name);

            if (variable == null)
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundLiteralExpression(0);
            }

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpression syntax)
        {
            var name            = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);

            var existingVariable = _variables.Keys.FirstOrDefault(v => v.Name == name);
            if (existingVariable != null)
                _variables.Remove(existingVariable);

            var variable = new VariableSymbol(name, boundExpression.Type);
            _variables[variable] = null;

            return new BoundAssignmentExpression(variable, boundExpression);
        }
    }
}
