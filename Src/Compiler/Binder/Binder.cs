using System.Collections.Immutable;
using Hyper.Compiler.Diagnostic;
using Hyper.Compiler.Parser;
using Hyper.Compiler.Symbols;
using Hyper.Compiler.Syntax;
using Hyper.Compiler.Syntax.Stmt;
using Hyper.Compiler.VM;

namespace Hyper.Compiler.Binding
{
    internal sealed class Binder
    {
        private          BoundScope?                        _scope;
        private readonly DiagnosticBag                      _diagnostics = new();
        private          IEnumerable<Diagnostic.Diagnostic> Diagnostics => _diagnostics;

        public Binder(BoundScope? parent)
        {
            _scope = new BoundScope(parent);
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnit unit)
        {
            var parentScope = CreateParentScope(previous);
            var binder      = new Binder(parentScope);

            var expression  = binder.BindStatement(unit.Statement);
            var variables   = binder._scope?.GetDeclaredVariables() ?? null;
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous != null)
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

            return new BoundGlobalScope(previous, diagnostics, variables, expression);
        }

        private static BoundScope? CreateParentScope(BoundGlobalScope? previous)
        {
            var stack = new Stack<BoundGlobalScope>();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            var parent = CreateRootScope();

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);

                if (previous.Variables != null)
                {
                    foreach (var v in previous.Variables)
                        scope.TryDeclareVariable(v);
                }

                parent = scope;
            }

            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            var result = new BoundScope(null);

            foreach (var f in BuiltinFunctions.GetAll())
                result.TryDeclareFunction(f);

            return result;
        }

        private BoundStatement? BindStatement(Statement syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.BlockStatement      => BindBlockStatement((BlockStatement) syntax),
                SyntaxKind.ExpressionStatement => BindExpressionStatement((ExpressionStatement) syntax),
                SyntaxKind.IfStatement         => BindIfStatement((IfStatement) syntax),
                SyntaxKind.DoWhileStatement    => BindDoWhileStatement((DoWhileStatement) syntax),
                SyntaxKind.WhileStatement      => BindWhileStatement((WhileStatement) syntax),
                SyntaxKind.ForStatement        => BindForStatement((ForStatement) syntax),
                SyntaxKind.VariableDeclaration => BindVariableDeclaration((VariableDeclaration) syntax),
                _                              => throw new Exception($"Unexpected syntax {syntax.Kind}")
            };
        }

        private BoundStatement BindBlockStatement(BlockStatement syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement?>();
            _scope = new BoundScope(_scope);

            foreach (var statement in syntax.Statements.Select(statementSyntax => BindStatement(statementSyntax)))
                statements.Add(statement);

            _scope = _scope.Parent;

            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundStatement BindExpressionStatement(ExpressionStatement syntax)
        {
            var expression = BindExpression(syntax.Expression, canBeVoid: true);
            return new BoundExpressionStatement(expression);
        }

        private BoundStatement BindVariableDeclaration(VariableDeclaration syntax)
        {
            var isReadOnly  = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
            var initializer = BindExpression(syntax.Initializer);
            var variable    = BindVariable(syntax.Identifier, isReadOnly, initializer.Type);

            return new BoundVariableDeclaration(variable, initializer);
        }

        private BoundStatement BindIfStatement(IfStatement syntax)
        {
            var condition     = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var thenStatement = BindStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);

            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }

        private BoundStatement BindDoWhileStatement(DoWhileStatement syntax)
        {
            var body      = BindStatement(syntax.Body);
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);

            return new BoundDoWhileStatement(body, condition);
        }

        private BoundStatement BindWhileStatement(WhileStatement syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var body      = BindStatement(syntax.Body);

            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindForStatement(ForStatement syntax)
        {
            var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
            var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

            _scope = new BoundScope(_scope);

            var variable = BindVariable(syntax.Identifier, true, TypeSymbol.Int);
            var body     = BindStatement(syntax.Body);

            _scope = _scope.Parent;

            return new BoundForStatement(variable, lowerBound, upperBound, body);
        }

        private BoundExpression BindExpression(Expression syntax, TypeSymbol targetType)
        {
            var result = BindExpression(syntax);
            if (targetType != TypeSymbol.Error && result.Type != TypeSymbol.Error && result.Type != targetType)
                _diagnostics.ReportCannotConvert(syntax.Span, result.Type, targetType);

            return result;
        }

        private BoundExpression BindExpression(Expression syntax, bool canBeVoid = false)
        {
            var result = BindExpressionInternal(syntax);

            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _diagnostics.ReportExpressionMustHaveValue(syntax.Span);
                return new BoundErrorExpression();
            }

            return result;
        }

        private BoundExpression BindExpressionInternal(Expression syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.LiteralExpression       => BindLiteralExpression((LiteralExpression) syntax),
                SyntaxKind.UnaryExpression         => BindUnaryExpression((UnaryExpression) syntax),
                SyntaxKind.BinaryExpression        => BindBinaryExpression((BinaryExpression) syntax),
                SyntaxKind.ParenthesizedExpression => BindParenthesizedExpression((ParenthesizedExpression) syntax),
                SyntaxKind.NameExpression          => BindNameExpression((NameExpression) syntax),
                SyntaxKind.CallExpression          => BindCallExpression((CallExpression) syntax),
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
            var boundOperand = BindExpression(syntax.Operand);
            if (boundOperand.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            var boundOperator = BoundUnaryOperator.Bind(syntax.Operator.Kind, boundOperand.Type);

            if (boundOperator != null)
                return new BoundUnaryExpression(boundOperator, boundOperand);

            _diagnostics.ReportUndefinedUnaryOperator(syntax.Operator.Span,
                                                      syntax.Operator.Text,
                                                      boundOperand.Type);
            return new BoundErrorExpression();
        }

        private BoundExpression BindBinaryExpression(BinaryExpression syntax)
        {
            var boundLeft     = BindExpression(syntax.Left);
            var boundRight    = BindExpression(syntax.Right);
            var boundOperator = BoundBinaryOperator.Bind(syntax.Operator.Kind, boundLeft.Type, boundRight.Type);

            if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            if (boundOperator != null)
                return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);

            _diagnostics.ReportUndefinedBinaryOperator(syntax.Operator.Span,
                                                       syntax.Operator.Text,
                                                       boundLeft.Type,
                                                       boundRight.Type);
            return new BoundErrorExpression();
        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpression syntax)
        {
            return BindExpression(syntax.Expression);
        }

        private BoundExpression BindNameExpression(NameExpression syntax)
        {
            var name = syntax.IdentifierToken.Text;
            if (syntax.IdentifierToken.IsMissing)
            {
                // This means the token was inserted by the parser. We already
                // reported error so we can just return an error expression.
                return new BoundErrorExpression();
            }

            if (!_scope.TryLookUpVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundErrorExpression();
            }

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpression syntax)
        {
            var name            = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);

            if (!_scope.TryLookUpVariable(name, out var variable))
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

        private BoundExpression BindCallExpression(CallExpression syntax)
        {
            if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is TypeSymbol type)
                return BindConversion(type, syntax.Arguments[0]);

            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (var arg in syntax.Arguments)
            {
                var boundArgument = BindExpression(arg);
                boundArguments.Add(boundArgument);
            }

            if (!_scope.TryLookUpFunction(syntax.Identifier.Text, out var function))
            {
                _diagnostics.ReportUndefinedFunction(syntax.Identifier.Span, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (syntax.Arguments.Count != function.Parameters.Length)
            {
                _diagnostics.ReportWrongArgumentCount(syntax.Span,
                                                      function.Name,
                                                      function.Parameters.Length,
                                                      syntax.Arguments.Count);
                return new BoundErrorExpression();
            }

            for (var i = 0; i < syntax.Arguments.Count; i++)
            {
                var argument  = boundArguments[i];
                var parameter = function.Parameters[i];

                if (argument.Type == parameter.Type)
                    continue;

                _diagnostics.ReportWrongArgumentType(syntax.Span, parameter.Name, parameter.Type, argument.Type);
                return new BoundErrorExpression();
            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindConversion(TypeSymbol type, Expression syntax)
        {
            var expression = BindExpression(syntax);
            var conversion = Conversion.Classify(expression.Type, type);

            if (!conversion.Exists)
            {
                _diagnostics.ReportCannotConvert(syntax.Span, expression.Type, type);
                return new BoundErrorExpression();
            }

            return new BoundConversionExpression(type, expression);
        }

        private VariableSymbol BindVariable(Token identifier, bool isReadOnly, TypeSymbol type)
        {
            var name     = identifier.Text ?? "?";
            var declare  = !identifier.IsMissing;
            var variable = new VariableSymbol(name, type, isReadOnly);

            if (declare && _scope != null && !_scope.TryDeclareVariable(variable))
                _diagnostics.ReportSymbolAlreadyDeclared(identifier.Span, name);

            return variable;
        }

        private TypeSymbol? LookupType(string? name)
        {
            return name switch
            {
                "bool"   => TypeSymbol.Bool,
                "int"    => TypeSymbol.Int,
                "string" => TypeSymbol.String,
                _        => null
            };
        }
    }
}
