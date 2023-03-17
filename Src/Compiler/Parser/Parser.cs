using System.Collections.Immutable;
using Hyper.Compiler.Diagnostic;
using Hyper.Compiler.Syntax;
using Hyper.Compiler.Syntax.Stmt;
using Hyper.Compiler.Text;
using Hyper.Compiler.VM;

namespace Hyper.Compiler.Parser
{
    internal sealed class Parser
    {
        private readonly SourceText            _text;
        private readonly ImmutableArray<Token> _tokens;
        private readonly DiagnosticBag         _diagnostics = new();
        private          int                   _position;

        public  DiagnosticBag Diagnostics => _diagnostics;
        private Token         Current     => Peek(0);

        public Parser(SourceText text)
        {
            var tokens = new List<Token>();
            var lexer  = new Lexer(text);

            Token token;
            do
            {
                token = lexer.Lex();

                if (token.Kind != SyntaxKind.WhitespaceToken && token.Kind != SyntaxKind.BadToken)
                    tokens.Add(token);
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _text = text;
            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        private Token Peek(int offset)
        {
            var index = _position + offset;
            return index >= _tokens.Length ? _tokens[^1] : _tokens[index];
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

            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
            return new Token(kind: kind, position: Current.Position);
        }

        public CompilationUnit ParseCompilationUnit()
        {
            var statement      = ParseStatement();
            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);

            return new CompilationUnit(statement, endOfFileToken);
        }


        private Statement ParseStatement()
        {
            return Current.Kind switch
            {
                SyntaxKind.OpenBraceToken                      => ParseBlockStatement(),
                SyntaxKind.LetKeyword or SyntaxKind.VarKeyword => ParseVariableDeclaration(),
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
                var statement = ParseStatement();
                statements.Add(statement);
            }

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new BlockStatement(openBraceToken, statements.ToImmutable(), closeBraceToken);
        }

        private Statement ParseVariableDeclaration()
        {
            var expected    = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
            var keyword     = Match(expected);
            var identifier  = Match(SyntaxKind.IdentifierToken);
            var equals      = Match(SyntaxKind.EqualsToken);
            var initializer = ParseExpression();

            return new VariableDeclaration(keyword, identifier, equals, initializer);
        }

        private ExpressionStatement ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionStatement(expression);
        }

        private Expression ParseBinaryExpression(int parentPrecedence = 0)
        {
            Expression left;

            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var opToken = NextToken();
                var operand = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new UnaryExpression(opToken, operand);
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

                left = new BinaryExpression(left, opToken, right);
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

                return new AssignmentExpression(identifierToken, operatorToken, right);
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
                SyntaxKind.IdentifierToken or _                   => ParseNameExpression(),
            };
        }

        private Expression ParseParenthesizedExpression()
        {
            var left       = NextToken();
            var expression = ParseExpression();
            var right      = Match(SyntaxKind.CloseParenthesisToken);

            return new ParenthesizedExpression(left, expression, right);
        }

        private Expression ParseBooleanLiteral()
        {
            var isTrue       = (Current.Kind == SyntaxKind.TrueKeyword);
            var keywordToken = isTrue ? Match(SyntaxKind.TrueKeyword) : Match(SyntaxKind.FalseKeyword);

            return new LiteralExpression(keywordToken, isTrue);
        }

        private Expression ParseNumberLiteral()
        {
            var numberToken = Match(SyntaxKind.NumberToken);
            return new LiteralExpression(numberToken);
        }

        private Expression ParseNameExpression()
        {
            var identifierToken = Match(SyntaxKind.IdentifierToken);
            return new NameExpression(identifierToken);
        }
    }
}
