﻿using Hyper.Compiler.Syntax;

namespace Hyper.Compiler.Parser
{
    internal sealed class Lexer
    {
        public Lexer(string text)
        {
            _text = text;
        }

        private void Next()
        {
            _position++;
        }

        public Token Lex()
        {
            if (_position >= _text.Length)
                return new Token(kind: SyntaxKind.EndOfFileToken, position: _position, text: "\0", value: null);

            if (char.IsDigit(Current))
            {
                int start = _position;

                while (char.IsDigit(Current))
                    Next();

                int length = _position - start;
                var text   = _text.Substring(start, length);

                if (!int.TryParse(text, out var value))
                    _diagnostics.Add($"The number {_text} isn't valid Int32.");

                return new Token(kind: SyntaxKind.NumberToken, position: start, text: text, value: value);
            }

            if (char.IsWhiteSpace(Current))
            {
                var start = _position;

                while (char.IsWhiteSpace(Current))
                    Next();

                var length = _position - start;
                var text   = _text.Substring(start, length);
                return new Token(SyntaxKind.WhitespaceToken, start, text);
            }

            if (char.IsLetter(Current))
            {
                var start = _position;
                while (char.IsLetter(Current))
                    Next();

                var length = _position - start;
                var text   = _text.Substring(start, length);
                var kind   = Factors.GetKeywordKind(text);

                return new Token(kind, start, text);
            }

            switch (Current)
            {
                case '+':
                    return new Token(SyntaxKind.PlusToken, _position++, "+");
                case '-':
                    return new Token(SyntaxKind.MinusToken, _position++, "-");
                case '*':
                    return new Token(SyntaxKind.StarToken, _position++, "*");
                case '/':
                    return new Token(SyntaxKind.SlashToken, _position++, "/");
                case '(':
                    return new Token(SyntaxKind.OpenParenthesisToken, _position++, "(");
                case ')':
                    return new Token(SyntaxKind.CloseParenthesisToken, _position++, ")");
                case '!':
                {
                    return Lookahead switch
                    {
                        '=' => new Token(SyntaxKind.EqualsEqualsToken, _position += 2, "!="),
                        _   => new Token(SyntaxKind.BangToken, _position++, "!")
                    };
                }
                case '=':
                {
                    if (Lookahead == '=')
                        return new Token(SyntaxKind.BangEqualsToken, _position += 2, "==");
                    break;
                }
                case '&':
                {
                    if (Lookahead == '&')
                        return new Token(SyntaxKind.AmpersandAmpersandToken, _position += 2, "&&");
                    break;
                }
                case '|':
                {
                    if (Lookahead == '|')
                        return new Token(SyntaxKind.PipePipeToken, _position += 2, "||");
                    break;
                }
            }

            _diagnostics.Add($"ERROR: bad character input: '{Current}'");
            return new Token(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1));
        }

        private char Peek(int offset)
        {
            var index = _position + offset;

            return index >= _text.Length ? '\0' : _text[index];
        }

        public IEnumerable<string> Diagnostics => _diagnostics;

        private char Current => Peek(0);

        private char Lookahead => Peek(1);


        private readonly string       _text;
        private          int          _position;
        private          List<string> _diagnostics = new();
    }
}
