using System.Collections.Immutable;
using Hyper.Core.Parser;
using Hyper.Core.Text;
using Hyper.Core.VM;

namespace Hyper.Core.Syntax;

public sealed class AST
{
    private AST(SourceText text)
    {
        var parser = new Parser.Parser(text);

        Root = parser.ParseCompilationUnit();
        Diagnostics = parser.Diagnostics.ToImmutableArray();
        Text = text;
    }

    public static AST Parse(SourceText text) => new(text);

    public static AST Parse(string text)
    {
        var sourceText = SourceText.MakeSTFrom(text);
        return Parse(sourceText);
    }

    public static ImmutableArray<Token> ParseTokens(string text)
    {
        var sourceText = SourceText.MakeSTFrom(text);
        return ParseTokens(sourceText);
    }

    public static ImmutableArray<Token> ParseTokens(SourceText text) => ParseTokens(text, out _);

    public static ImmutableArray<Token> ParseTokens(string text, out ImmutableArray<Diagnostic.Diagnostic> diagnostics)
    {
        var sourceText = SourceText.MakeSTFrom(text);
        return ParseTokens(sourceText, out diagnostics);
    }

    public static ImmutableArray<Token> ParseTokens(SourceText text,
                                                    out ImmutableArray<Diagnostic.Diagnostic> diagnostics)
    {
        IEnumerable<Token> LexTokens(Lexer lexer)
        {
            while (true)
            {
                var token = lexer.Lex();
                if (token.Kind == SyntaxKind.EndOfFileToken)
                    break;

                yield return token;
            }
        }

        var l      = new Lexer(text);
        var result = LexTokens(l).ToImmutableArray();
        diagnostics = l.Diagnostics.ToImmutableArray();
        return result;
    }

    public CompilationUnit                       Root        { get; }
    public ImmutableArray<Diagnostic.Diagnostic> Diagnostics { get; }
    public SourceText                            Text;
}
