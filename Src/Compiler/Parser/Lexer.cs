using Hyper.Compiler.Diagnostic;
using Hyper.Compiler.Syntax;
using Hyper.Compiler.Text;

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

            int start = _position;
            if (char.IsDigit(Current))
            {
                while (char.IsDigit(Current))
                    Next();

                int length = _position - start;
                var text   = _text.Substring(start, length);

                if (!int.TryParse(text, out var value))
                    _diagnostics.ReportInvalidNumber(new TextSpan(start, length), _text, typeof(int));

                return new Token(kind: SyntaxKind.NumberToken, position: start, text: text, value: value);
            }

            if (char.IsWhiteSpace(Current))
            {
                while (char.IsWhiteSpace(Current))
                    Next();

                var length = _position - start;
                var text   = _text.Substring(start, length);
                return new Token(SyntaxKind.WhitespaceToken, start, text);
            }

            if (char.IsLetter(Current))
            {
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
                    if (Lookahead == '=')
                    {
                        _position += 2;
                        return new Token(SyntaxKind.EqualsEqualsToken, start, "!=");
                    }
                    else
                    {
                        _position++;
                        return new Token(SyntaxKind.BangToken, start, "!");
                    }
                }
                case '=':
                {
                    if (Lookahead == '=')
                    {
                        _position += 2;
                        return new Token(SyntaxKind.BangEqualsToken, start, "==");
                    }

                    break;
                }
                case '&':
                {
                    if (Lookahead == '&')
                    {
                        _position += 2;
                        return new Token(SyntaxKind.AmpersandAmpersandToken, start, "&&");
                    }

                    break;
                }
                case '|':
                {
                    if (Lookahead == '|')
                    {
                        _position += 2;
                        return new Token(SyntaxKind.PipePipeToken, start, "||");
                    }

                    break;
                }
            }

            _diagnostics.ReportBadCharacter(_position, Current);
            return new Token(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1));
        }

        private char Peek(int offset)
        {
            var index = _position + offset;

            return index >= _text.Length ? '\0' : _text[index];
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        private char Current => Peek(0);

        private char Lookahead => Peek(1);


        private readonly string        _text;
        private          int           _position;
        private          DiagnosticBag _diagnostics = new();
    }
}
