using Hyper.Compiler.Diagnostic;
using Hyper.Compiler.Syntax;

namespace Hyper.Compiler.Parser
{
    internal sealed class Parser
    {
        private readonly Token[]       _tokens;
        private readonly DiagnosticBag _diagnostics = new();
        private          int           _position;

        public  DiagnosticBag Diagnostics => _diagnostics;
        private Token         Current     => Peek(0);

        public Parser(string text)
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

            _tokens = tokens.ToArray();
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

        public AST Parse()
        {
            var expression     = ParseExpression();
            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);

            return new AST(expression, endOfFileToken, _diagnostics);
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
            var keywordToken = NextToken();
            var value        = (keywordToken.Kind == SyntaxKind.TrueKeyword);

            return new LiteralExpression(keywordToken, value);
        }

        private Expression ParseNumberLiteral()
        {
            var numberToken = Match(SyntaxKind.NumberToken);
            return new LiteralExpression(numberToken);
        }

        private Expression ParseNameExpression()
        {
            var identifierToken = NextToken();
            return new NameExpression(identifierToken);
        }
    }
}
