﻿using Hyper.Core.IO;
using Hyper.Core.Symbols;
using Hyper.Core.Syntax;
using Hyper.Core.VM;

namespace HyperI;

internal sealed class HyperREPL : REPL
{
    private          Compilation?                       _previous;
    private          bool                               _showTree;
    private          bool                               _showProgram;
    private static   bool                               _loadingSubmission;
    private readonly Dictionary<VariableSymbol, object> _variables = new();

    public HyperREPL() => LoadSubmissions();

    protected override void RenderLine(string line)
    {
        var tokens = AST.ParseTokens(line);

        foreach (var token in tokens)
        {
            var isKeyword    = token.Kind.ToString().EndsWith("Keyword");
            var isNumber     = token.Kind == SyntaxKind.NumberToken;
            var isIdentifier = token.Kind == SyntaxKind.IdentifierToken;
            var isString     = token.Kind == SyntaxKind.StringToken;

            if (isKeyword)
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (isNumber)
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
            else if (isString)
                Console.ForegroundColor = ConsoleColor.Magenta;
            else if (isIdentifier)
                Console.ForegroundColor = ConsoleColor.DarkYellow;

            Console.Write(token.Text);
            Console.ResetColor();
        }
    }

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
        var compilation = _previous ?? new Compilation();
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
        var compilation = _previous ?? new Compilation();

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
        if (syntaxTree.Root.Members.Last().GetLastToken().IsMissing)
            return false;

        return true;
    }

    protected override void EvaluateSubmission(string text)
    {
        var ast = AST.Parse(text);

        var compilation = _previous == null ? new Compilation(ast) : _previous.ContinueWith(ast);

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

    private static void ClearSubmissions() => Directory.Delete(GetSubmissionsDirectory(), recursive: true);

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

    private static void SaveSubmissions(string text)
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
