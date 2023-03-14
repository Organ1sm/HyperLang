using System.Collections.Immutable;
using System.Security.Cryptography;
using Hyper.Compiler.Parser;
using Hyper.Compiler.Diagnostic;

namespace Hyper.Compiler.Syntax;

public sealed class AST
{
    public AST(Expression root, Token eofToken, ImmutableArray<Diagnostic.Diagnostic> diagnostics)
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

    public static IEnumerable<Token> ParseTokens(string text)
    {
        var lexer = new Lexer(text);
        while (true)
        {
            var token = lexer.Lex();
            if (token.Kind == SyntaxKind.EndOfFileToken)
                break;

            yield return token;
        }
    }

    public Expression                            Root        { get; }
    public Token                                 EOFToken    { get; }
    public ImmutableArray<Diagnostic.Diagnostic> Diagnostics { get; }
}
