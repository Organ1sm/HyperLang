using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax;

public sealed class AST
{
    public AST(Expression root, Token eofToken, IReadOnlyList<string> diagnostics)
    {
        Root = root;
        EOFToken = eofToken;
        Diagnostics = diagnostics;
    }

    public static AST Parse(string text)
    {
        var parser = new Parser.Parser(text);
        return parser.Parse();
    }

    public Expression            Root        { get; }
    public Token                 EOFToken    { get; }
    public IReadOnlyList<string> Diagnostics { get; }
}
