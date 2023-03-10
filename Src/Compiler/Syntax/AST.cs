using Hyper.Compiler.Parser;
using Hyper.Compiler.Diagnostic;

namespace Hyper.Compiler.Syntax;

public sealed class AST
{
    public AST(Expression root, Token eofToken, IEnumerable<Diagnostic.Diagnostic> diagnostics)
    {
        Root = root;
        EOFToken = eofToken;
        Diagnostics = diagnostics.ToArray();
    }

    public static AST Parse(string text)
    {
        var parser = new Parser.Parser(text);
        return parser.Parse();
    }

    public Expression                           Root        { get; }
    public Token                                EOFToken    { get; }
    public IReadOnlyList<Diagnostic.Diagnostic> Diagnostics { get; }
}
