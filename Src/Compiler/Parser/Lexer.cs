using Hyper.Compiler.Diagnostic;
using Hyper.Compiler.Syntax;
using Hyper.Compiler.Text;

namespace Hyper.Compiler.Parser
{
    internal sealed class Lexer
    {
        public Lexer(SourceText text)
        {
            _text = text;
        }

        public Token Lex()
        {
            _start = _position;
            _kind = SyntaxKind.BadToken;
            _value = null;

            switch (Current)
            {
                case '\0':
                    _kind = SyntaxKind.EndOfFileToken;
                    break;
                case '+':
                    _kind = SyntaxKind.PlusToken;
                    _position++;
                    break;
                case '-':
                    _kind = SyntaxKind.MinusToken;
                    _position++;
                    break;
                case '*':
                    _kind = SyntaxKind.StarToken;
                    _position++;
                    break;
                case '/':
                    _kind = SyntaxKind.SlashToken;
                    _position++;
                    break;
                case '(':
                    _kind = SyntaxKind.OpenParenthesisToken;
                    _position++;
                    break;
                case ')':
                    _kind = SyntaxKind.CloseParenthesisToken;
                    _position++;
                    break;
                case '!':
                {
                    if (Lookahead == '=')
                    {
                        _position += 2;
                        _kind = SyntaxKind.BangEqualsToken;
                    }
                    else
                    {
                        _position++;
                        _kind = SyntaxKind.BangToken;
                    }

                    break;
                }
                case '=':
                {
                    if (Lookahead == '=')
                    {
                        _kind = SyntaxKind.EqualsEqualsToken;
                        _position += 2;
                        break;
                    }

                    _position++;
                    _kind = SyntaxKind.EqualsToken;
                    break;
                }
                case '&':
                {
                    if (Lookahead == '&')
                    {
                        _kind = SyntaxKind.AmpersandAmpersandToken;
                        _position += 2;
                    }

                    break;
                }
                case '|':
                {
                    if (Lookahead == '|')
                    {
                        _kind = SyntaxKind.PipePipeToken;
                        _position += 2;
                        break;
                    }

                    break;
                }
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    LexNumber();
                    break;
                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    LexWhiteSpace();
                    break;

                default:
                    if (char.IsLetter(Current))
                        LexIdentifierOrKeyword();
                    else if (char.IsWhiteSpace(Current))
                        LexWhiteSpace();
                    else
                    {
                        _diagnostics.ReportBadCharacter(_position, Current);
                        _position++;
                    }

                    break;
            }

            var length = _position - _start;
            var text   = Factors.GetText(_kind) ?? _text.ToString(_start, length);

            return new Token(_kind, _start, text, _value);
        }

        private void LexNumber()
        {
            while (char.IsDigit(Current))
                _position++;

            var length = _position - _start;
            var text   = _text.ToString(_start, length);

            if (!int.TryParse(text, out var value))
                _diagnostics.ReportInvalidNumber(new TextSpan(_start, length), text, typeof(int));

            _value = value;
            _kind = SyntaxKind.NumberToken;
        }

        private void LexWhiteSpace()
        {
            while (char.IsWhiteSpace(Current))
                _position++;

            _kind = SyntaxKind.WhitespaceToken;
        }

        private void LexIdentifierOrKeyword()
        {
            while (char.IsLetter(Current))
                _position++;

            var length = _position - _start;
            var text   = _text.ToString(_start, length);
            _kind = Factors.GetKeywordKind(text);
        }

        private char Peek(int offset)
        {
            var index = _position + offset;

            return index >= _text.Length ? '\0' : _text[index];
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        private char Current => Peek(0);

        private char Lookahead => Peek(1);

        private readonly SourceText    _text;
        private          int           _position;
        private          DiagnosticBag _diagnostics = new();

        private int        _start;
        private SyntaxKind _kind;
        private object     _value;
    }
}
