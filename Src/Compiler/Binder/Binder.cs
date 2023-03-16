using System.Collections.Immutable;
using System.Diagnostics.SymbolStore;
using Hyper.Compiler.Diagnostic;
using Hyper.Compiler.Symbol;
using Hyper.Compiler.Syntax;
using Hyper.Compiler.VM;

namespace Hyper.Compiler.Binding
{
    internal sealed class Binder
    {
        private          BoundScope                         _scope;
        private readonly DiagnosticBag                      _diagnostics = new();
        public           IEnumerable<Diagnostic.Diagnostic> Diagnostics => _diagnostics;

        public Binder(BoundScope parent)
        {
            _scope = new BoundScope(parent);
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnit unit)
        {
            var parentScope = CreateParentScope(previous);
            var binder      = new Binder(parentScope);

            var expression  = binder.BindExpression(unit.Expression);
            var variables   = binder._scope.GetDeclaredVariables();
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous != null)
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

            return new BoundGlobalScope(previous, diagnostics, variables, expression);
        }

        private static BoundScope CreateParentScope(BoundGlobalScope previous)
        {
            var stack = new Stack<BoundGlobalScope>();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            BoundScope parent = null;

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);

                foreach (var v in previous.Variables)
                    scope.TryDeclare(v);

                parent = scope;
            }

            return parent;
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
            var name = syntax.IdentifierToken.Text;

            if (!_scope.TryLookUp(name, out var variable))
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

            if (!_scope.TryLookUp(name, out var variable))
            {
                variable = new VariableSymbol(name, boundExpression.Type);
                _scope.TryDeclare(variable);
            }

            if (boundExpression.Type != variable.Type)
            {
                _diagnostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
                return boundExpression;
            }

            return new BoundAssignmentExpression(variable, boundExpression);
        }
    }
}
