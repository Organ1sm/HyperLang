using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Operator;
using Hyper.Core.Binding.Opt;
using Hyper.Core.Binding.Scope;
using Hyper.Core.Binding.Stmt;
using Hyper.Core.Symbols;
using Hyper.Core.Syntax;
using Hyper.Core.Text;
using Hyper.Core.Diagnostic;
using Hyper.Core.Lowering;
using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;
using Hyper.Core.Syntax.Stmt;
using Microsoft.VisualBasic;
using Conversion = Hyper.Core.Binding.Expr.Conversion;

namespace Hyper.Core.Binding
{
    internal sealed class Binder
    {
        private          BoundScope      _scope;
        private readonly bool            _isScript;
        private readonly FunctionSymbol? _function;
        private readonly DiagnosticBag   _diagnostics = new();
        private          DiagnosticBag   Diagnostics => _diagnostics;

        private Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> _loopStack = new();

        private int _labelCounter;

        private Binder(bool isScript, BoundScope? parent, FunctionSymbol? function)
        {
            _scope = new BoundScope(parent);
            _isScript = isScript;
            _function = function;

            if (function == null) return;

            foreach (var p in function.Parameters)
                _scope.TryDeclareVariable(p);
        }

        public static BoundProgram BindProgram(bool isScript, BoundProgram? previous, BoundGlobalScope globalScope)
        {
            var parentScope = CreateParentScope(globalScope);

            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
            var diagnostics    = ImmutableArray.CreateBuilder<Diagnostic.Diagnostic>();

            foreach (var function in globalScope.Functions)
            {
                var binder      = new Binder(isScript, parentScope, function);
                var body        = binder.BindStatement(function.Declaration!.Body);
                var loweredBody = Lowerer.Lower(function, body);

                if (function.Type != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(loweredBody))
                    binder._diagnostics.ReportAllPathsMustReturn(function.Declaration!.Identifier.Location);

                functionBodies.Add(function, loweredBody);
                diagnostics.AddRange(binder.Diagnostics);
            }

            if (globalScope.MainFunction != null && globalScope.Statements.Any())
            {
                var body = Lowerer.Lower(globalScope.MainFunction, new BoundBlockStatement(globalScope.Statements));
                functionBodies.Add(globalScope.MainFunction, body);
            }
            else if (globalScope.ScriptFunction != null)
            {
                var statements = globalScope.Statements;

                if (statements.Length == 1 &&
                    statements[0] is BoundExpressionStatement es &&
                    es.Expression.Type != TypeSymbol.Void)
                {
                    statements = statements.SetItem(0, new BoundReturnStatement(es.Expression));
                }
                else if (statements.Any() && statements.Last().Kind != BoundNodeKind.ReturnStatement)
                {
                    var nullValue = new BoundLiteralExpression("");
                    statements = statements.Add(new BoundReturnStatement(nullValue));
                }

                var body = Lowerer.Lower(globalScope.ScriptFunction, new BoundBlockStatement(statements));
                functionBodies.Add(globalScope.ScriptFunction, body);
            }


            return new BoundProgram(previous,
                                    diagnostics.ToImmutable(),
                                    globalScope.MainFunction,
                                    globalScope.ScriptFunction,
                                    functionBodies.ToImmutable());
        }

        public static BoundGlobalScope BindGlobalScope(bool isScript,
                                                       BoundGlobalScope? previous,
                                                       ImmutableArray<AST> syntaxTrees)
        {
            var parentScope = CreateParentScope(previous);
            var binder      = new Binder(isScript, parentScope, function: null);

            binder.Diagnostics.AddRange(syntaxTrees.SelectMany(st => st.Diagnostics));
            if (binder.Diagnostics.Any())
                return new BoundGlobalScope(previous,
                                            binder.Diagnostics.ToImmutableArray(),
                                            null,
                                            null,
                                            ImmutableArray<VariableSymbol>.Empty,
                                            ImmutableArray<FunctionSymbol>.Empty,
                                            ImmutableArray<BoundStatement>.Empty);


            var functionDeclarations = syntaxTrees.SelectMany(st => st.Root.Members)
                                                  .OfType<FunctionDeclaration>();

            foreach (var function in functionDeclarations)
                binder.BindFunctionDeclaration(function);

            var globalStatements = syntaxTrees.SelectMany(st => st.Root.Members)
                                              .OfType<GlobalStatement>();
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();

            var enumerable = globalStatements.ToList();
            foreach (var s in enumerable.Select(globalStatement =>
                                                    binder.BindGlobalStatement(globalStatement.Statement)))
                statements.Add(s);

            // Check global Statement

            var firstGlobalStatementSyntaxTree = syntaxTrees
                                                 .Select(st => st.Root.Members.OfType<GlobalStatement>()
                                                                 .FirstOrDefault())
                                                 .Where(g => g != null)
                                                 .ToArray();

            if (firstGlobalStatementSyntaxTree.Length > 1)
            {
                foreach (var globalStatement in firstGlobalStatementSyntaxTree)
                    binder.Diagnostics.ReportOnlyOneFileCanHaveGlobalStatements(globalStatement!.Location);
            }

            // Check for main/script with global statement..
            var functions = binder._scope.GetDeclaredFunctions();

            FunctionSymbol? mainFunction;
            FunctionSymbol? scriptFunction;

            if (isScript)
            {
                mainFunction = null;
                if (enumerable.Any())
                    scriptFunction =
                        new FunctionSymbol("$eval", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Any);
                else
                    scriptFunction = null;
            }
            else
            {
                mainFunction = functions.FirstOrDefault(f => f.Name == "main");
                scriptFunction = null;
                if (mainFunction != null)
                {
                    if (mainFunction.Type != TypeSymbol.Void || mainFunction.Parameters.Any())
                        binder.Diagnostics.ReportMainMustHaveCorrectSignature(mainFunction.Declaration!.Location);
                }

                if (enumerable.Any())
                {
                    if (mainFunction != null)
                    {
                        binder.Diagnostics.ReportCannotMixMainAndGlobalStatements(mainFunction.Declaration!.Identifier
                            .Location);

                        foreach (var globalStatement in firstGlobalStatementSyntaxTree)
                            binder.Diagnostics.ReportCannotMixMainAndGlobalStatements(globalStatement!.Location);
                    }
                    else
                    {
                        mainFunction =
                            new FunctionSymbol("main", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void);
                    }
                }
            }

            var variables   = binder._scope.GetDeclaredVariables();
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous != null)
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

            return new BoundGlobalScope(previous,
                                        diagnostics,
                                        mainFunction,
                                        scriptFunction,
                                        variables,
                                        functions,
                                        statements.ToImmutable());
        }

        private static BoundScope CreateParentScope(BoundGlobalScope? previous)
        {
            var stack = new Stack<BoundGlobalScope>();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            var parent = CreateRootScope();

            // Pass the symbol from global scope to the local scope.
            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);

                foreach (var f in previous.Functions)
                    scope.TryDeclareFunction(f);

                foreach (var v in previous.Variables)
                    scope.TryDeclareVariable(v);

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

        private void BindFunctionDeclaration(FunctionDeclaration syntax)
        {
            var parameters         = ImmutableArray.CreateBuilder<ParameterSymbol>();
            var seenParameterNames = new HashSet<string?>();

            foreach (var parameter in syntax.Parameters)
            {
                var parameterName = parameter.Identifier.Text;
                var parameterType = BindTypeClause(parameter.Type);

                if (!seenParameterNames.Add(parameterName))
                {
                    _diagnostics.ReportParameterAlreadyDeclared(parameter.Location, parameterName);
                }
                else
                {
                    var p = new ParameterSymbol(parameterName, parameterType, parameters.Count);
                    parameters.Add(p);
                }
            }

            var type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;

            var function = new FunctionSymbol(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax);
            if (!_scope.TryDeclareFunction(function))
                _diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Location, function.Name);
        }

        private BoundStatement BindGlobalStatement(Statement syntax) => BindStatement(syntax, isGlobal: true);

        private BoundStatement BindStatement(Statement syntax, bool isGlobal = false)
        {
            var result = BindStatementInternal(syntax);

            if (!_isScript || !isGlobal)
            {
                if (result is BoundExpressionStatement es)
                {
                    var isAllowedExpression = es.Expression.Kind == BoundNodeKind.ErrorExpression ||
                                              es.Expression.Kind == BoundNodeKind.AssignmentExpression ||
                                              es.Expression.Kind == BoundNodeKind.CallExpression;
                    if (!isAllowedExpression)
                        _diagnostics.ReportInvalidExpressionStatement(syntax.Location);
                }
            }

            return result;
        }

        private BoundStatement BindStatementInternal(Statement syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.BlockStatement      => BindBlockStatement((BlockStatement) syntax),
                SyntaxKind.ExpressionStatement => BindExpressionStatement((ExpressionStatement) syntax),
                SyntaxKind.IfStatement         => BindIfStatement((IfStatement) syntax),
                SyntaxKind.DoWhileStatement    => BindDoWhileStatement((DoWhileStatement) syntax),
                SyntaxKind.WhileStatement      => BindWhileStatement((WhileStatement) syntax),
                SyntaxKind.ForStatement        => BindForStatement((ForStatement) syntax),
                SyntaxKind.BreakStatement      => BindBreakStatement((BreakStatement) syntax),
                SyntaxKind.ContinueStatement   => BindContinueStatement((ContinueStatement) syntax),
                SyntaxKind.ReturnStatement     => BindReturnStatement((ReturnStatement) syntax),
                SyntaxKind.VariableDeclaration => BindVariableDeclaration((VariableDeclaration) syntax),
                _                              => throw new Exception($"Unexpected syntax {syntax.Kind}")
            };
        }

        private BoundStatement BindErrorStatement() => new BoundExpressionStatement(new BoundErrorExpression());

        private BoundStatement BindBlockStatement(BlockStatement syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);

            foreach (var statement in syntax.Statements.Select(statementSyntax => BindStatement(statementSyntax)))
                statements.Add(statement);

            _scope = _scope.Parent!;

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
            var type        = BindTypeClause(syntax.TypeClause);
            var initializer = BindExpression(syntax.Initializer);
            var varType     = type ?? initializer.Type;
            var variable    = BindVariableDeclaration(syntax.Identifier, isReadOnly, varType);

            var convertedInitializer = BindConversion(syntax.Initializer.Location, initializer, varType);

            return new BoundVariableDeclaration(variable, convertedInitializer);
        }

        [return: NotNullIfNotNull("syntax")]
        private TypeSymbol? BindTypeClause(TypeClause? syntax)
        {
            if (syntax == null)
                return null;

            var type = LookupType(syntax.Identifier.Text);
            if (type == null)
                _diagnostics.ReportUndefinedType(syntax.Identifier.Location, syntax.Identifier.Text);

            return type;
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
            var body      = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);

            return new BoundDoWhileStatement(body, condition, breakLabel, continueLabel);
        }

        private BoundStatement BindWhileStatement(WhileStatement syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var body      = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

            return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
        }

        private BoundStatement BindForStatement(ForStatement syntax)
        {
            var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
            var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

            _scope = new BoundScope(_scope);

            var variable = BindVariableDeclaration(syntax.Identifier, false, TypeSymbol.Int);
            var body     = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

            _scope = _scope.Parent!;

            return new BoundForStatement(variable, lowerBound, upperBound, body, breakLabel, continueLabel);
        }

        private BoundStatement BindLoopBody(Statement body, out BoundLabel breakLabel, out BoundLabel continueLabel)
        {
            _labelCounter++;
            breakLabel = new($"break{_labelCounter}");
            continueLabel = new($"continue{_labelCounter}");

            _loopStack.Push((breakLabel, continueLabel));
            var boundBody = BindStatement(body);
            _loopStack.Pop();

            return boundBody;
        }

        private BoundStatement BindBreakStatement(BreakStatement syntax)
        {
            if (_loopStack.Count == 0)
            {
                _diagnostics.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BindErrorStatement();
            }

            var breakLabel = _loopStack.Peek().BreakLabel;
            return new BoundGotoStatement(breakLabel);
        }

        private BoundStatement BindContinueStatement(ContinueStatement syntax)
        {
            if (_loopStack.Count == 0)
            {
                _diagnostics.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BindErrorStatement();
            }

            var continueLabel = _loopStack.Peek().ContinueLabel;
            return new BoundGotoStatement(continueLabel);
        }

        private BoundStatement BindReturnStatement(ReturnStatement syntax)
        {
            var expression = syntax.Expression == null ? null : BindExpression(syntax.Expression);

            if (_function == null)
            {
                // Ignore because we allow both return with and without values.
                if (_isScript)
                    expression ??= new BoundLiteralExpression("");

                // Main does not support return values.
                else if (expression != null)
                    _diagnostics.ReportInvalidReturnWithValueInGlobalStatements(syntax.Expression!.Location);
            }
            else
            {
                if (_function.Type == TypeSymbol.Void)
                {
                    if (expression != null && syntax.Expression != null)
                        _diagnostics.ReportInvalidReturnExpression(syntax.Expression!.Location, _function.Name);
                }
                else
                {
                    if (expression == null)
                        _diagnostics.ReportMissingReturnExpression(syntax.ReturnKeyword.Location, _function.Type);
                    else if (syntax.Expression != null)
                        expression = BindConversion(syntax.Expression!.Location, expression, _function.Type);
                }
            }

            return new BoundReturnStatement(expression);
        }

        private BoundExpression BindExpression(Expression syntax, TypeSymbol targetType) =>
            BindConversion(targetType, syntax);

        private BoundExpression BindExpression(Expression syntax, bool canBeVoid = false)
        {
            var result = BindExpressionInternal(syntax);

            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _diagnostics.ReportExpressionMustHaveValue(syntax.Location);
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
            var value = syntax.Value;

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

            _diagnostics.ReportUndefinedUnaryOperator(syntax.Operator.Location,
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

            _diagnostics.ReportUndefinedBinaryOperator(syntax.Operator.Location,
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
            if (syntax.IdentifierToken.IsMissing)
            {
                // This means the token was inserted by the parser. We already
                // reported error so we can just return an error expression.
                return new BoundErrorExpression();
            }

            var variable = BindVariableReference(syntax.IdentifierToken);
            if (variable == null)
                return new BoundErrorExpression();

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpression syntax)
        {
            var name            = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);

            var variable = BindVariableReference(syntax.IdentifierToken);
            switch (variable)
            {
                case null:
                    return boundExpression;
                case {IsReadOnly: true}:
                    _diagnostics.ReportCannotAssign(syntax.EqualsToken.Location, name);
                    break;
            }

            var convertedExpression = BindConversion(syntax.Expression.Location, boundExpression, variable.Type);

            return new BoundAssignmentExpression(variable, convertedExpression);
        }

        private BoundExpression BindCallExpression(CallExpression syntax)
        {
            if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is { } type)
                return BindConversion(type, syntax.Arguments[0], allowExplicit: true);

            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (var arg in syntax.Arguments)
            {
                var boundArgument = BindExpression(arg);
                boundArguments.Add(boundArgument);
            }

            var symbol = _scope.TryLookUpSymbol(syntax.Identifier.Text);
            if (symbol == null)
            {
                _diagnostics.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            var function = symbol as FunctionSymbol;
            if (function == null)
            {
                _diagnostics.ReportNotAFunction(syntax.Identifier.Location, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (syntax.Arguments.Count != function.Parameters.Length)
            {
                TextSpan span;
                if (syntax.Arguments.Count > function.Parameters.Length)
                {
                    Node? firstExceedingNode;
                    if (function.Parameters.Length > 0)
                        firstExceedingNode = syntax.Arguments.GetSeparator(function.Parameters.Length - 1);
                    else
                        firstExceedingNode = syntax.Arguments[0];

                    var lastExceedingNode = syntax.Arguments[^1];

                    span = TextSpan.MakeTextSpanFromBound(firstExceedingNode.Span.Start, lastExceedingNode.Span.End);
                }
                else
                {
                    span = syntax.CloseParenthesisToken.Span;
                }

                var location = new TextLocation(syntax.SyntaxTree.Text, span);
                _diagnostics.ReportWrongArgumentCount(location,
                                                      function.Name,
                                                      function.Parameters.Length,
                                                      syntax.Arguments.Count);
                return new BoundErrorExpression();
            }

            for (var i = 0; i < syntax.Arguments.Count; i++)
            {
                var argumentLocation = syntax.Arguments[i].Location;
                var argument         = boundArguments[i];
                var parameter        = function.Parameters[i];

                boundArguments[i] = BindConversion(argumentLocation, argument, parameter.Type);
            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindConversion(TypeSymbol type, Expression syntax, bool allowExplicit = false)
        {
            var expression = BindExpression(syntax);
            return BindConversion(syntax.Location, expression, type, allowExplicit);
        }

        private BoundExpression BindConversion(TextLocation diagnosticLocation,
                                               BoundExpression expression,
                                               TypeSymbol type,
                                               bool allowExplicit = false)
        {
            var conversion = Conversion.Classify(expression.Type, type);

            if (!allowExplicit && conversion.IsExplicit)
                _diagnostics.ReportCannotConvertImplicitly(diagnosticLocation, expression.Type, type);

            if (conversion.Exists)
                return conversion.IsIdentity ? expression : new BoundConversionExpression(type, expression);

            if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                _diagnostics.ReportCannotConvert(diagnosticLocation, expression.Type, type);

            return new BoundErrorExpression();
        }

        private VariableSymbol BindVariableDeclaration(Token identifier, bool isReadOnly, TypeSymbol type)
        {
            var name    = identifier.Text;
            var declare = !identifier.IsMissing;
            var variable = _function == null
                ? (VariableSymbol) new GlobalVariableSymbol(name, type, isReadOnly)
                : new LocalVariableSymbol(name, type, isReadOnly);

            if (declare && !_scope.TryDeclareVariable(variable))
                _diagnostics.ReportSymbolAlreadyDeclared(identifier.Location, name);

            return variable;
        }

        private VariableSymbol? BindVariableReference(Token identifier)
        {
            var name = identifier.Text;
            switch (_scope.TryLookUpSymbol(name))
            {
                case VariableSymbol variable:
                    return variable;

                case null:
                    _diagnostics.ReportUndefinedVariable(identifier.Location, name);
                    return null;

                default:
                    _diagnostics.ReportNotAVariable(identifier.Location, name);
                    return null;
            }
        }

        private TypeSymbol? LookupType(string? name)
        {
            return name switch
            {
                "bool"   => TypeSymbol.Bool,
                "int"    => TypeSymbol.Int,
                "string" => TypeSymbol.String,
                "any"    => TypeSymbol.Any,

                _ => null
            };
        }
    }
}
