using System.Collections.Immutable;
using Hyper.Compiler.Diagnostic;
using Hyper.Compiler.Symbol;
using Hyper.Compiler.Syntax;
using Hyper.Compiler.Syntax.Stmt;
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

            var expression  = binder.BindStatement(unit.Statement);
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

        private BoundStatement BindStatement(Statement syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.BlockStatement      => BindBlockStatement((BlockStatement) syntax),
                SyntaxKind.ExpressionStatement => BindExpressionStatement((ExpressionStatement) syntax),
                SyntaxKind.IfStatement         => BindIfStatement((IfStatement) syntax),
                SyntaxKind.WhileStatement      => BindWhileStatement((WhileStatement) syntax),
                SyntaxKind.ForStatement        => BindForStatement((ForStatement) syntax),
                SyntaxKind.VariableDeclaration => BindVariableDeclaration((VariableDeclaration) syntax),
                _                              => throw new Exception($"Unexpected syntax {syntax.Kind}")
            };
        }

        private BoundStatement BindBlockStatement(BlockStatement syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);

            foreach (var statementSyntax in syntax.Statements)
            {
                var statement = BindStatement(statementSyntax);
                statements.Add(statement);
            }

            _scope = _scope.Parent;

            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundStatement BindExpressionStatement(ExpressionStatement syntax)
        {
            var expression = BindExpression(syntax.Expression);
            return new BoundExpressionStatement(expression);
        }

        private BoundStatement BindVariableDeclaration(VariableDeclaration syntax)
        {
            var name        = syntax.Identifier.Text;
            var isReadOnly  = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
            var initializer = BindExpression(syntax.Initializer);
            var variable    = new VariableSymbol(name, initializer.Type, isReadOnly);

            if (!_scope.TryDeclare(variable))
                _diagnostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);

            return new BoundVariableDeclaration(variable, initializer);
        }

        private BoundStatement BindIfStatement(IfStatement syntax)
        {
            var condition     = BindExpression(syntax.Condition, typeof(bool));
            var thenStatement = BindStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);

            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }

        private BoundStatement BindWhileStatement(WhileStatement syntax)
        {
            var condition = BindExpression(syntax.Condition, typeof(bool));
            var body      = BindStatement(syntax.Body);

            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindForStatement(ForStatement syntax)
        {
            var lowerBound = BindExpression(syntax.LowerBound, typeof(int));
            var upperBound = BindExpression(syntax.UpperBound, typeof(int));

            _scope = new BoundScope(_scope);

            var name     = syntax.Identifier.Text;
            var variable = new VariableSymbol(name, typeof(int), true);
            if (!_scope.TryDeclare(variable))
                _diagnostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);

            var body = BindStatement(syntax.Body);

            _scope = _scope.Parent;

            return new BoundForStatement(variable, lowerBound, upperBound, body);
        }

        private BoundExpression BindExpression(Expression syntax, Type targetType)
        {
            var result = BindExpression(syntax);
            if (result.Type != targetType)
                _diagnostics.ReportCannotConvert(syntax.Span, result.Type, targetType);

            return result;
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
            if (string.IsNullOrEmpty(name))
            {
                // This means the token was inserted by the parser. We already
                // reported error so we can just return an error expression.
                return new BoundLiteralExpression(0);
            }

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
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return boundExpression;
            }

            if (variable.IsReadOnly)
                _diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);

            if (boundExpression.Type != variable.Type)
            {
                _diagnostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
                return boundExpression;
            }

            return new BoundAssignmentExpression(variable, boundExpression);
        }
    }
}
