using System.Collections.Immutable;
using Hyper.Core.Diagnostic;
using Hyper.Core.Syntax;
using Hyper.Core.Syntax.Expr;
using Hyper.Core.Syntax.Stmt;
using Hyper.Core.Text;
using Hyper.Core.VM;

namespace Hyper.Core.Parser
{
    internal sealed class Parser
    {
        private readonly AST                   _syntaxTree;
        private readonly SourceText            _text;
        private readonly ImmutableArray<Token> _tokens;
        private readonly DiagnosticBag         _diagnostics = new();
        private          int                   _position;

        public  DiagnosticBag Diagnostics => _diagnostics;
        private Token         Current     => Peek(0);

        public Parser(AST syntaxTree)
        {
            var tokens = new List<Token>();
            var lexer  = new Lexer(syntaxTree);

            Token token;
            do
            {
                token = lexer.Lex();

                if (token.Kind != SyntaxKind.WhitespaceToken && token.Kind != SyntaxKind.BadToken &&
                    token.Kind != SyntaxKind.SingleLineCommentToken && token.Kind != SyntaxKind.MultiLineCommentToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _syntaxTree = syntaxTree;
            _text = syntaxTree.Text;
            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        private Token Peek(int offset)
        {
            var index = _position + offset;
            return index >= _tokens.Length ? _tokens[_tokens.Length - 1] : _tokens[index];
        }

        private Token NextToken()
        {
            var current = Current;
            _position++;

            return current;
        }

        private Token Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            _diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, kind);
            return new Token(_syntaxTree, kind: kind, position: Current.Position);
        }

        public CompilationUnit ParseCompilationUnit()
        {
            var members        = ParseMembers();
            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);

            return new CompilationUnit(_syntaxTree, members, endOfFileToken);
        }

        private ImmutableArray<MemberSyntax> ParseMembers()
        {
            var members = ImmutableArray.CreateBuilder<MemberSyntax>();

            while (Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var startToken = Current;

                var member = ParseMember();
                members.Add(member);

                // If ParseMember() did not consume any tokens,
                // we need to skip the current token and continue
                // in order to avoid an infinite loop.
                //
                // We don't need to report an error, because we'll
                // already tried to parse an expression statement
                // and reported one.
                if (Current == startToken)
                    NextToken();
            }

            return members.ToImmutable();
        }

        private MemberSyntax ParseMember()
        {
            return Current.Kind == SyntaxKind.FuncKeyword ? ParseFunctionDeclaration() : ParseGlobalStatement();
        }

        private MemberSyntax ParseFunctionDeclaration()
        {
            var funcKeyword      = Match(SyntaxKind.FuncKeyword);
            var identifier       = Match(SyntaxKind.IdentifierToken);
            var openParentToken  = Match(SyntaxKind.OpenParenthesisToken);
            var parameters       = ParseParameterList();
            var closeParentToken = Match(SyntaxKind.CloseParenthesisToken);
            var type             = ParseOptionalTypeClause();
            var body             = ParseBlockStatement();

            return new FunctionDeclaration(_syntaxTree,
                                           funcKeyword,
                                           identifier,
                                           openParentToken,
                                           parameters,
                                           closeParentToken,
                                           type,
                                           body);
        }

        private SeparatedSyntaxList<Parameter> ParseParameterList()
        {
            var nodeAndSeparators = ImmutableArray.CreateBuilder<Node>();

            var parseNextParameter = true;
            while (parseNextParameter && Current.Kind != SyntaxKind.CloseParenthesisToken &&
                   Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var parameter = ParseParameter();
                nodeAndSeparators.Add(parameter);

                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    var comma = Match(SyntaxKind.CommaToken);
                    nodeAndSeparators.Add(comma);
                }
                else
                {
                    parseNextParameter = false;
                }
            }

            return new SeparatedSyntaxList<Parameter>(nodeAndSeparators.ToImmutable());
        }

        private Parameter ParseParameter()
        {
            var identifier = Match(SyntaxKind.IdentifierToken);
            var type       = ParseTypeClause();

            return new Parameter(_syntaxTree, identifier, type);
        }

        private MemberSyntax ParseGlobalStatement()
        {
            var statement = ParseStatement();
            return new GlobalStatement(_syntaxTree, statement);
        }

        private Statement ParseStatement()
        {
            return Current.Kind switch
            {
                SyntaxKind.OpenBraceToken                      => ParseBlockStatement(),
                SyntaxKind.LetKeyword or SyntaxKind.VarKeyword => ParseVariableDeclaration(),
                SyntaxKind.IfKeyword                           => ParseIfStatement(),
                SyntaxKind.WhileKeyword                        => ParseWhileStatement(),
                SyntaxKind.DoKeyword                           => ParseDoWhileStatement(),
                SyntaxKind.ForKeyword                          => ParseForStatement(),
                SyntaxKind.BreakKeyword                        => ParseBreakStatement(),
                SyntaxKind.ContinueKeyword                     => ParseContinueStatement(),
                SyntaxKind.ReturnKeyword                       => ParseReturnStatement(),
                _                                              => ParseExpressionStatement()
            };
        }

        private BlockStatement ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<Statement>();

            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            while (Current.Kind != SyntaxKind.EndOfFileToken &&
                   Current.Kind != SyntaxKind.CloseBraceToken)
            {
                var startToken = Current;

                var statement = ParseStatement();
                statements.Add(statement);

                // If ParseStatement() did not consume any tokens,
                // we need to skip the current token and continue
                // in order to avoid an infinite loop.
                //
                // We don't need to report an error, because we'll
                // already tried to parse an expression statement
                // and reported one.
                if (Current == startToken)
                    NextToken();
            }

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new BlockStatement(_syntaxTree, openBraceToken, statements.ToImmutable(), closeBraceToken);
        }

        private Statement ParseVariableDeclaration()
        {
            var expected    = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
            var keyword     = Match(expected);
            var identifier  = Match(SyntaxKind.IdentifierToken);
            var typeClause  = ParseOptionalTypeClause();
            var equals      = Match(SyntaxKind.EqualsToken);
            var initializer = ParseExpression();

            return new VariableDeclaration(_syntaxTree, keyword, identifier, typeClause, equals, initializer);
        }

        private TypeClause? ParseOptionalTypeClause()
        {
            if (Current.Kind != SyntaxKind.ArrowToken && Current.Kind != SyntaxKind.ColonToken)
                return null;

            return ParseTypeClause();
        }

        private TypeClause ParseTypeClause()
        {
            var token = Current.Kind switch
            {
                SyntaxKind.ColonToken => Match(SyntaxKind.ColonToken),
                SyntaxKind.ArrowToken => Match(SyntaxKind.ArrowToken),
            };
            var identifier = Match(SyntaxKind.IdentifierToken);

            return new TypeClause(_syntaxTree, token, identifier);
        }

        private Statement ParseIfStatement()
        {
            var keyword   = Match(SyntaxKind.IfKeyword);
            var condition = ParseExpression();
            Match(SyntaxKind.ColonToken);
            var statement  = ParseStatement();
            var elseClause = ParseOptionalElseClause();

            return new IfStatement(_syntaxTree, keyword, condition, statement, elseClause);
        }

        private Statement ParseDoWhileStatement()
        {
            var doKeyword    = Match(SyntaxKind.DoKeyword);
            var _            = Match(SyntaxKind.ColonToken);
            var body         = ParseStatement();
            var whileKeyword = Match(SyntaxKind.WhileKeyword);
            var condition    = ParseExpression();

            return new DoWhileStatement(_syntaxTree, doKeyword, body, whileKeyword, condition);
        }

        private Statement ParseWhileStatement()
        {
            var keyword   = Match(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            Match(SyntaxKind.ColonToken);
            var body = ParseStatement();

            return new WhileStatement(_syntaxTree, keyword, condition, body);
        }

        private ElseClause? ParseOptionalElseClause()
        {
            if (Current.Kind != SyntaxKind.ElseKeyword)
                return null;

            var keyword = NextToken();
            Match(SyntaxKind.ColonToken);
            var statement = ParseStatement();

            return new ElseClause(_syntaxTree, keyword, statement);
        }

        private Statement ParseForStatement()
        {
            var keyword     = Match(SyntaxKind.ForKeyword);
            var identifier  = Match(SyntaxKind.IdentifierToken);
            var equalsToken = Match(SyntaxKind.EqualsToken);
            var lowerBound  = ParseExpression();
            var toKeyword   = Match(SyntaxKind.ToKeyword);
            var upperBound  = ParseExpression();
            var body        = ParseStatement();

            return new ForStatement(_syntaxTree,
                                    keyword,
                                    identifier,
                                    equalsToken,
                                    lowerBound,
                                    toKeyword,
                                    upperBound,
                                    body);
        }

        private Statement ParseBreakStatement()
        {
            var keyword = Match(SyntaxKind.BreakKeyword);
            return new BreakStatement(_syntaxTree, keyword);
        }

        private Statement ParseContinueStatement()
        {
            var keyword = Match(SyntaxKind.ContinueKeyword);
            return new ContinueStatement(_syntaxTree, keyword);
        }

        private Statement ParseReturnStatement()
        {
            var keyword     = Match(SyntaxKind.ReturnKeyword);
            var keywordLine = _text.GetLineIndex(keyword.Span.Start);
            var currentLine = _text.GetLineIndex(Current.Span.Start);
            var isEof       = Current.Kind == SyntaxKind.EndOfFileToken;
            var sameLine    = !isEof && keywordLine == currentLine;
            var expression  = sameLine ? ParseExpression() : null;

            return new ReturnStatement(_syntaxTree, keyword, expression);
        }

        private ExpressionStatement ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionStatement(_syntaxTree, expression);
        }

        private Expression ParseBinaryExpression(int parentPrecedence = 0)
        {
            Expression left;

            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var opToken = NextToken();
                var operand = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new UnaryExpression(_syntaxTree, opToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }

            while (true)
            {
                var precedence = Current.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;

                var opToken = NextToken();
                var right   = ParseBinaryExpression(precedence);

                left = new BinaryExpression(_syntaxTree, left, opToken, right);
            }

            return left;
        }

        private Expression ParseExpression()
        {
            return ParseAssignmentExpression();
        }

        private Expression ParseAssignmentExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.EqualsToken)
            {
                var identifierToken = NextToken();
                var operatorToken   = NextToken();
                var right           = ParseAssignmentExpression();

                return new AssignmentExpression(_syntaxTree, identifierToken, operatorToken, right);
            }

            return ParseBinaryExpression();
        }

        private Expression ParsePrimaryExpression()
        {
            return Current.Kind switch
            {
                SyntaxKind.OpenParenthesisToken                   => ParseParenthesizedExpression(),
                SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword => ParseBooleanLiteral(),
                SyntaxKind.NumberToken                            => ParseNumberLiteral(),
                SyntaxKind.StringToken                            => ParseStringLiteral(),
                SyntaxKind.IdentifierToken or _                   => ParseNameOrCallExpression(),
            };
        }

        private Expression ParseParenthesizedExpression()
        {
            var left       = NextToken();
            var expression = ParseExpression();
            var right      = Match(SyntaxKind.CloseParenthesisToken);

            return new ParenthesizedExpression(_syntaxTree, left, expression, right);
        }

        private Expression ParseStringLiteral()
        {
            var stringToken = Match(SyntaxKind.StringToken);
            return new LiteralExpression(_syntaxTree, stringToken);
        }

        private Expression ParseBooleanLiteral()
        {
            var isTrue       = (Current.Kind == SyntaxKind.TrueKeyword);
            var keywordToken = isTrue ? Match(SyntaxKind.TrueKeyword) : Match(SyntaxKind.FalseKeyword);

            return new LiteralExpression(_syntaxTree, keywordToken, isTrue);
        }

        private Expression ParseNumberLiteral()
        {
            var numberToken = Match(SyntaxKind.NumberToken);
            return new LiteralExpression(_syntaxTree, numberToken);
        }

        private Expression ParseNameOrCallExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.OpenParenthesisToken)
                return ParseCallExpression();

            return ParseNameExpression();
        }

        private Expression ParseCallExpression()
        {
            var identifier  = Match(SyntaxKind.IdentifierToken);
            var openParent  = Match(SyntaxKind.OpenParenthesisToken);
            var arguments   = ParseArguments();
            var closeParent = Match(SyntaxKind.CloseParenthesisToken);

            return new CallExpression(_syntaxTree, identifier, openParent, arguments, closeParent);
        }

        private SeparatedSyntaxList<Expression> ParseArguments()
        {
            var nodeAndSeparators = ImmutableArray.CreateBuilder<Node>();
            var parseNextArgument = true;

            while (parseNextArgument && Current.Kind != SyntaxKind.CloseParenthesisToken &&
                   Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var expression = ParseExpression();
                nodeAndSeparators.Add(expression);

                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    var comma = Match(SyntaxKind.CommaToken);
                    nodeAndSeparators.Add(comma);
                }
                else
                {
                    parseNextArgument = false;
                }
            }

            return new SeparatedSyntaxList<Expression>(nodeAndSeparators.ToImmutable());
        }

        private Expression ParseNameExpression()
        {
            var identifierToken = Match(SyntaxKind.IdentifierToken);
            return new NameExpression(_syntaxTree, identifierToken);
        }
    }
}
