namespace HyperLang.SimpleDemo;

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
            default:
                _diagnostics.Add($"ERROR: bad character input: '{Current}'");
                return new Token(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1));
        }
    }

    public IEnumerable<string> Diagnostics => _diagnostics;

    private char Current
    {
        get
        {
            if (_position >= _text.Length)
                return '\0';

            return _text[_position];
        }
    }

    private readonly string       _text;
    private          int          _position;
    private          List<string> _diagnostics = new();
}
