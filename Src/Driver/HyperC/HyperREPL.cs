using Hyper.Compiler.Parser;
using Hyper.Compiler.Symbol;
using Hyper.Compiler.Syntax;
using Hyper.Compiler.Text;
using Hyper.Compiler.VM;

namespace Hyper;

internal sealed class HyperREPL : REPL
{
    private          Compilation                        _previous;
    private          bool                               _showTree;
    private          bool                               _showProgram;
    private readonly Dictionary<VariableSymbol, object> _variables = new Dictionary<VariableSymbol, object>();

    protected override void RenderLine(string line)
    {
        var tokens = AST.ParseTokens(line);

        foreach (var token in tokens)
        {
            var isKeyword    = token.Kind.ToString().EndsWith("Keyword");
            var isNumber     = token.Kind == SyntaxKind.NumberToken;
            var isIdentifier = token.Kind == SyntaxKind.IdentifierToken;

            if (isKeyword)
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (isNumber)
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
            else if (isIdentifier)
                Console.ForegroundColor = ConsoleColor.DarkYellow;

            Console.Write(token.Text);
            Console.ResetColor();
        }
    }

    protected override void EvaluateMetaCommand(string input)
    {
        switch (input)
        {
            case "#showTree":
                _showTree = !_showTree;
                Console.WriteLine(_showTree ? "Showing parse trees." : "Not showing parse trees.");
                break;
            case "#showProgram":
                _showProgram = !_showProgram;
                Console.WriteLine(_showProgram ? "Showing bound tree." : "Not showing bound tree.");
                break;
            case "#reset":
                _previous = null;
                _variables.Clear();
                break;
            default:
                base.EvaluateMetaCommand(input);
                break;
        }
    }

    protected override bool IsCompleteSubmission(string text)
    {
        if (string.IsNullOrEmpty(text))
            return true;

        var syntaxTree = AST.Parse(text);

        // Use Statement because we need to exclude the EndOfFileToken.
        if (syntaxTree.Root.Statement.GetLastToken().IsMissing)
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
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(result.Value);
            Console.ResetColor();
            _previous = compilation;
        }
        else
        {
            foreach (var diagnostic in result.Diagnostics)
            {
                var lineIndex  = ast.Text.GetLineIndex(diagnostic.Span.Start);
                var line       = ast.Text.Lines[lineIndex];
                var lineNumber = lineIndex + 1;
                var character  = diagnostic.Span.Start - line.Start + 1;

                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write($"({lineNumber}, {character}): ");
                Console.WriteLine(diagnostic);
                Console.ResetColor();

                var prefixSpan = TextSpan.MakeTextSpanFromBound(line.Start, diagnostic.Span.Start);
                var suffixSpan = TextSpan.MakeTextSpanFromBound(diagnostic.Span.End, line.End);

                var prefix = ast.Text.ToString(prefixSpan);
                var error  = ast.Text.ToString(diagnostic.Span);
                var suffix = ast.Text.ToString(suffixSpan);

                Console.Write("     ");
                Console.Write(prefix);

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write(error);
                Console.ResetColor();

                Console.Write(suffix);

                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}
