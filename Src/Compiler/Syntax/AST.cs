using System.Collections.Immutable;
using System.Security.Cryptography;
using Hyper.Compiler.Parser;
using Hyper.Compiler.Diagnostic;
using Hyper.Compiler.Text;

namespace Hyper.Compiler.Syntax;

public sealed class AST
{
    public AST(Expression root, Token eofToken, ImmutableArray<Diagnostic.Diagnostic> diagnostics, SourceText text)
    {
        Root = root;
        EOFToken = eofToken;
        Diagnostics = diagnostics;
        Text = text;
    }

    public static AST Parse(SourceText text)
    {
        var parser = new Parser.Parser(text);
        return parser.Parse();
    }

    public static AST Parse(string text)
    {
        var sourceText = SourceText.MakeSTFrom(text);
        return Parse(sourceText);
    }

    public static IEnumerable<Token> ParseTokens(string text)
    {
        var sourceText = SourceText.MakeSTFrom(text);
        return ParseTokens(sourceText);
    }

    public static IEnumerable<Token> ParseTokens(SourceText text)
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
    public SourceText                            Text;
}
