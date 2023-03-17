using System.Collections.Immutable;
using Hyper.Compiler.Parser;
using Hyper.Compiler.Text;
using Hyper.Compiler.VM;

namespace Hyper.Compiler.Syntax;

public sealed class AST
{
    private AST(SourceText text)
    {
        var parser      = new Parser.Parser(text);
        var root        = parser.ParseCompilationUnit();
        var diagnostics = parser.Diagnostics.ToImmutableArray();

        Root = root;
        Diagnostics = diagnostics;
        Text = text;
    }

    public static AST Parse(SourceText text) => new AST(text);

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

    public CompilationUnit                       Root        { get; }
    public ImmutableArray<Diagnostic.Diagnostic> Diagnostics { get; }
    public SourceText                            Text;
}
