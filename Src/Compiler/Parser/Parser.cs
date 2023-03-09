﻿using Hyper.Compiler.Syntax;

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
            var expresion      = ParseExpression();
            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);

            return new AST(expresion, endOfFileToken, _diagnostics);
        }

        private Expression ParseExpression(int parentPrecedence = 0)
        {
            Expression left;

            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var opToken = NextToken() ?? throw new ArgumentNullException("NextToken()");
                var operand = ParseExpression(unaryOperatorPrecedence);
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
                var right   = ParseExpression();

                left = new BinaryExpression(left, opToken, right);
            }

            return left;
        }


        private Expression ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                {
                    var left       = NextToken();
                    var expression = ParseExpression();
                    var right      = Match(SyntaxKind.CloseParenthesisToken);

                    return new ParenthesizedExpression(left, expression, right);
                }

                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                {
                    var keywordToken = NextToken();
                    var value        = (keywordToken.Kind == SyntaxKind.TrueKeyword);

                    return new LiteralExpression(keywordToken, value);
                }

                default:
                {
                    var numberToken = Match(SyntaxKind.NumberToken);
                    return new LiteralExpression(numberToken);
                }
            }
        }
    }
}
