using Hyper.Compiler.Syntax;

namespace Hyper.Compiler.Parser
{
    internal sealed class Parser
    {
        private readonly Token[]      _tokens;
        private          List<string> _diagnostics = new();
        private          int          _position;

        public  IEnumerable<string> Diagnostics => _diagnostics;
        private Token               Current     => Peek(0);

        public Parser(string text)
        {
            var tokens = new List<Token>();
            var lexer  = new Lexer(text);

            Token token;
            do
            {
                token = lexer.Lex();

                if (token.Kind != SyntaxKind.WhitespaceToken && token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _tokens = tokens.ToArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        private Token Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens[_tokens.Length - 1];

            return _tokens[index];
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

            _diagnostics.Add($"ERROR: Unexpected token <{Current.Kind}>, expected <{kind}>");
            return new Token(kind: kind, position: Current.Position, null);
        }

        public AST Parse()
        {
            var expresion      = ParseTerm();
            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);

            return new AST(expresion, endOfFileToken, _diagnostics);
        }

        private Expression ParseExpression(int parentPrecedence = 0)
        {
            Expression left;

            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var OPToken = NextToken();
                var operand = ParseExpression(unaryOperatorPrecedence);
                left = new UnaryExpression(OPToken, operand);
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

                var OPToken = NextToken();
                var right   = ParseExpression();

                left = new BinaryExpression(left, OPToken, right);
            }

            return left;
        }

        private Expression ParseTerm()
        {
            var left = ParseFactor();

            while (Current.Kind == SyntaxKind.PlusToken ||
                   Current.Kind == SyntaxKind.MinusToken)
            {
                var operatorToken = NextToken();
                var right         = ParseFactor();

                left = new BinaryExpression(left, operatorToken, right);
            }

            return left;
        }

        private Expression ParseFactor()
        {
            var left = ParsePrimaryExpression();
            while (Current.Kind == SyntaxKind.StarToken ||
                   Current.Kind == SyntaxKind.SlashToken)
            {
                var operatorToken = NextToken();
                var right         = ParsePrimaryExpression();

                left = new BinaryExpression(left, operatorToken, right);
            }

            return left;
        }

        private Expression ParsePrimaryExpression()
        {
            if (Current.Kind == SyntaxKind.OpenParenthesisToken)
            {
                var left       = NextToken();
                var expression = ParseExpression();
                var right      = Match(SyntaxKind.CloseParenthesisToken);
                return new ParenthesizedExpression(left, expression, right);
            }

            var numberToken = Match(SyntaxKind.NumberToken);
            return new LiteralExpression(numberToken);
        }
    }
}
