using System.Collections.Immutable;
using Hyper.Core.IO;
using Hyper.Core.Parser;
using Hyper.Core.Symbols;
using Hyper.Core.Syntax;
using Hyper.Core.Text;
using Hyper.Core.VM;

namespace HyperI;

internal sealed class HyperREPL : REPL
{
    private                 Compilation?                       _previous;
    private                 bool                               _showTree;
    private                 bool                               _showProgram;
    private                 bool                               _loadingSubmission;
    private static readonly Compilation                        EmptyCompilation = Compilation.CreateScript(null);
    private readonly        Dictionary<VariableSymbol, object> _variables       = new();

    public HyperREPL() => LoadSubmissions();

    private sealed class RenderState
    {
        public RenderState(SourceText text, ImmutableArray<Token> tokens)
        {
            Text = text;
            Tokens = tokens;
        }

        public SourceText            Text   { get; }
        public ImmutableArray<Token> Tokens { get; }
    }

    protected override object? RenderLine(IReadOnlyList<string> lines, int lineIndex, object? state)
    {
        RenderState renderState;

        if (state == null)
        {
            var text       = string.Join(Environment.NewLine, lines);
            var sourceText = SourceText.MakeSTFrom(text);
            var tokens     = AST.ParseTokens(sourceText);
            renderState = new RenderState(sourceText, tokens);
        }
        else
        {
            renderState = (RenderState) state;
        }

        var lineSpan = renderState.Text.Lines[lineIndex].Span;
        foreach (var token in renderState.Tokens)
        {
            if (!lineSpan.OverlapsWith(token.Span))
                continue;

            var tokenStart = Math.Max(token.Span.Start, lineSpan.Start);
            var tokenEnd   = Math.Min(token.Span.End, lineSpan.End);
            var tokenSpan  = TextSpan.MakeTextSpanFromBound(tokenStart, tokenEnd);
            var tokenText  = renderState.Text.ToString(tokenSpan);

            var isKeyword    = token.Kind.ToString().EndsWith("Keyword");
            var isNumber     = token.Kind == SyntaxKind.NumberToken;
            var isIdentifier = token.Kind == SyntaxKind.IdentifierToken;
            var isString     = token.Kind == SyntaxKind.StringToken;
            var isComment    = token.Kind.IsComment();

            if (isKeyword)
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (isNumber)
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
            else if (isString)
                Console.ForegroundColor = ConsoleColor.Magenta;
            else if (isIdentifier)
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            else if (isComment)
                Console.ForegroundColor = ConsoleColor.Green;

            Console.Write(tokenText);
            Console.ResetColor();
        }

        return renderState;
    }

    [MetaCommand("exit", "Exits the REPL")]
    private void EvaluateExit() => Environment.Exit(0);

    [MetaCommand("cls", "Clears the screen")]
    private void EvaluateCls() => Console.Clear();

    [MetaCommand("reset", "Clears all previous submissions")]
    private void EvaluateReset()
    {
        _previous = null;
        _variables.Clear();
        ClearSubmissions();
    }

    [MetaCommand("showTree", "Shows the parse tree")]
    private void EvaluateShowTree()
    {
        _showTree = !_showTree;
        Console.WriteLine(_showTree ? "Showing parse trees." : "Not showing parse trees.");
    }

    [MetaCommand("showProgram", "Shows the bound tree")]
    private void EvaluateShowProgram()
    {
        _showProgram = !_showProgram;
        Console.WriteLine(_showProgram ? "Showing bound tree." : "Not showing bound tree.");
    }

    [MetaCommand("load", "Loads a script file")]
    private void EvaluateLoad(string path)
    {
        path = Path.GetFullPath(path);
        if (!File.Exists(path))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"error: file does not exist '{path}'");
            Console.ResetColor();
            return;
        }

        var text = File.ReadAllText(path);
        EvaluateSubmission(text);
    }


    [MetaCommand("ls", "List all symbols")]
    private void EvaluateLs()
    {
        var compilation = _previous ?? EmptyCompilation;
        var symbols = compilation.GetSymbols()
                                 .OrderBy(s => s.Kind)
                                 .ThenBy(s => s.Name);

        foreach (var symbol in symbols)
        {
            symbol.WriteTo(Console.Out);
            Console.WriteLine();
        }
    }

    [MetaCommand("dump", "Shows bound tree of a given function")]
    private void EvaluateDump(string functionName)
    {
        var compilation = _previous ?? EmptyCompilation;
        var symbols = compilation.GetSymbols()
                                 .OfType<FunctionSymbol>()
                                 .SingleOrDefault(f => f.Name == functionName);
        if (symbols == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"error: function '{functionName}' does not exist");
            Console.ResetColor();
            return;
        }

        compilation.EmitTree(symbols, Console.Out);
    }

    protected override bool IsCompleteSubmission(string text)
    {
        if (string.IsNullOrEmpty(text))
            return true;

        var lastTwoLinesAreBlank = text.Split(Environment.NewLine)
                                       .Reverse()
                                       .TakeWhile(s => string.IsNullOrEmpty(s))
                                       .Take(2)
                                       .Count() == 2;

        if (lastTwoLinesAreBlank)
            return true;

        var syntaxTree = AST.Parse(text);

        // Use Statement because we need to exclude the EndOfFileToken.
        var lastMember = syntaxTree.Root.Members.LastOrDefault();
        return lastMember == null || !syntaxTree.Root.Members.Last().GetLastToken().IsMissing;
    }

    protected override void EvaluateSubmission(string text)
    {
        var ast         = AST.Parse(text);
        var compilation = Compilation.CreateScript(_previous, ast);

        if (_showTree)
            ast.Root.WriteTo(Console.Out);

        if (_showProgram)
            compilation.EmitTree(Console.Out);

        var result = compilation.Evaluate(_variables);

        if (!result.Diagnostics.Any())
        {
            if (result.Value != null)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(result.Value);
                Console.ResetColor();
            }

            _previous = compilation;

            SaveSubmissions(text);
        }
        else
        {
            Console.Out.WriteDiagnostics(result.Diagnostics);
        }
    }

    private static string GetSubmissionsDirectory()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "HyperLang",
                        "Submissions");

    private static void ClearSubmissions()
    {
        var dir = GetSubmissionsDirectory();
        if (Directory.Exists(dir))
            Directory.Delete(GetSubmissionsDirectory(), recursive: true);
    }

    private void LoadSubmissions()
    {
        var submissionsDirectory = GetSubmissionsDirectory();
        if (!Directory.Exists(submissionsDirectory)) return;

        var files = Directory.GetFiles(submissionsDirectory).OrderBy(f => f).ToArray();
        if (files.Length == 0)
            return;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"Loaded {files.Length} submission(s)");
        Console.ResetColor();

        _loadingSubmission = true;

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            EvaluateSubmission(text);
        }

        _loadingSubmission = false;
    }

    private void SaveSubmissions(string text)
    {
        if (_loadingSubmission)
            return;

        var submissionsDirectory = GetSubmissionsDirectory();
        Directory.CreateDirectory(submissionsDirectory);

        var count    = Directory.GetFiles(submissionsDirectory).Length;
        var name     = $"submission{count:0000}";
        var fileName = Path.Combine(submissionsDirectory, name);

        File.WriteAllText(fileName, text);
    }
}
