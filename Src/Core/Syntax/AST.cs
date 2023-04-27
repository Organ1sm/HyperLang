using System.Collections.Immutable;
using Hyper.Core.Parser;
using Hyper.Core.Syntax.Stmt;
using Hyper.Core.Text;
using Hyper.Core.VM;

namespace Hyper.Core.Syntax;

public sealed class AST
{
    private delegate void ParseHandler(AST syntaxTree,
                                       out CompilationUnit root,
                                       out ImmutableArray<Diagnostic.Diagnostic> diagnostics);

    private AST(SourceText text, ParseHandler handler)
    {
        Text = text;
        handler(this, out var root, out var diagnostics);

        Diagnostics = diagnostics;
        Root = root;
    }

    public static AST Load(string fileName)
    {
        var text       = File.ReadAllText(fileName);
        var sourceText = SourceText.MakeSTFrom(text, fileName);

        return Parse(sourceText);
    }

    private static void Parse(AST syntaxTree,
                              out CompilationUnit root,
                              out ImmutableArray<Diagnostic.Diagnostic> diagnostics)
    {
        var parser = new Parser.Parser(syntaxTree);

        root = parser.ParseCompilationUnit();
        diagnostics = parser.Diagnostics.ToImmutableArray();
    }

    public static AST Parse(SourceText text) => new(text, Parse);

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
        var tokens = new List<Token>();

        void ParseTokens(AST st, out CompilationUnit root, out ImmutableArray<Diagnostic.Diagnostic> d)
        {
            var l = new Lexer(st);
            while (true)
            {
                var token = l.Lex();
                if (token.Kind == SyntaxKind.EndOfFileToken)
                {
                    root = new CompilationUnit(st, ImmutableArray<MemberSyntax>.Empty, token);
                    break;
                }

                tokens.Add(token);
            }

            d = l.Diagnostics.ToImmutableArray();
        }

        var syntaxTree = new AST(text, ParseTokens);
        diagnostics = syntaxTree.Diagnostics.ToImmutableArray();
        return tokens.ToImmutableArray();
    }

    public CompilationUnit                       Root        { get; }
    public ImmutableArray<Diagnostic.Diagnostic> Diagnostics { get; }
    public SourceText                            Text;
}
