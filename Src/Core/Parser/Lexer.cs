using System.Text;
using Hyper.Core.Symbols;
using Hyper.Core.Syntax;
using Hyper.Core.Diagnostic;
using Hyper.Core.Text;

namespace Hyper.Core.Parser
{
    internal sealed class Lexer
    {
        public Lexer(AST syntaxTree)
        {
            _syntaxTree = syntaxTree;
            _text = syntaxTree.Text;
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
                {
                    _position++;
                    if (Current == '>')
                    {
                        _position++;
                        _kind = SyntaxKind.ArrowToken;
                        break;
                    }

                    _kind = SyntaxKind.MinusToken;
                    break;
                }
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
                case '{':
                    _kind = SyntaxKind.OpenBraceToken;
                    _position++;
                    break;
                case ':':
                    _kind = SyntaxKind.ColonToken;
                    _position++;
                    break;
                case '}':
                    _kind = SyntaxKind.CloseBraceToken;
                    _position++;
                    break;
                case ',':
                    _kind = SyntaxKind.CommaToken;
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
                case '~':
                    _kind = SyntaxKind.TildeToken;
                    _position++;
                    break;

                case '^':
                    _kind = SyntaxKind.HatToken;
                    _position++;
                    break;

                case '&':
                {
                    if (Lookahead == '&')
                    {
                        _kind = SyntaxKind.AmpersandAmpersandToken;
                        _position += 2;
                        break;
                    }

                    _position++;
                    _kind = SyntaxKind.AmpersandToken;

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

                    _position++;
                    _kind = SyntaxKind.PipeToken;

                    break;
                }
                case '<':
                {
                    _position++;
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.LessToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.LessOrEqualsToken;
                        _position++;
                    }

                    break;
                }
                case '>':
                {
                    _position++;
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.GreaterToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.GreaterOrEqualsToken;
                        _position++;
                    }

                    break;
                }

                case '"':
                    LexString();
                    break;
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

                case '_':
                    LexIdentifierOrKeyword();
                    break;

                default:
                    if (char.IsLetter(Current))
                        LexIdentifierOrKeyword();
                    else if (char.IsWhiteSpace(Current))
                        LexWhiteSpace();
                    else
                    {
                        var span     = new TextSpan(_position, 1);
                        var location = new TextLocation(_text, span);

                        _diagnostics.ReportBadCharacter(location, Current);
                        _position++;
                    }

                    break;
            }

            var length = _position - _start;
            var text   = Factors.GetText(_kind) ?? _text.ToString(_start, length);

            return new Token(_syntaxTree, _kind, _start, text, _value);
        }

        private void LexString()
        {
            _position++; // eat '"'

            var sb   = new StringBuilder();
            var done = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                    {
                        var span     = new TextSpan(_start, 1);
                        var location = new TextLocation(_text, span);

                        _diagnostics.ReportUnterminatedString(location);
                        done = true;
                        break;
                    }

                    case '"':
                    {
                        if (Lookahead == '"')
                        {
                            sb.Append(Current);
                            _position += 2;
                        }
                        else
                        {
                            _position++;
                            done = true;
                        }

                        break;
                    }

                    default:
                        sb.Append(Current);
                        _position++;
                        break;
                }
            }

            _kind = SyntaxKind.StringToken;
            _value = sb.ToString();
        }

        private void LexNumber()
        {
            while (char.IsDigit(Current))
                _position++;

            var length = _position - _start;
            var text   = _text.ToString(_start, length);

            if (!int.TryParse(text, out var value))
            {
                var span     = new TextSpan(_start, length);
                var location = new TextLocation(_text, span);

                _diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Int);
            }

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
            while (char.IsLetterOrDigit(Current) || Current == '_')
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

        private readonly AST           _syntaxTree;
        private readonly SourceText    _text;
        private          int           _position;
        private          DiagnosticBag _diagnostics = new();

        private int        _start;
        private SyntaxKind _kind;
        private object?    _value;
    }
}
