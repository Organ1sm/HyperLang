using System.Text;
using Hyper.Compiler.Parser;
using Hyper.Compiler.Symbol;
using Hyper.Compiler.Syntax;
using Hyper.Compiler.Text;

namespace Hyper
{
    class Program
    {
        static void Main(string[] args)
        {
            bool showTree    = false;
            var  variables   = new Dictionary<VariableSymbol, object>();
            var  textBuilder = new StringBuilder();

            while (true)
            {
                Console.Write(textBuilder.Length == 0 ? "> " : "| ");

                var input   = Console.ReadLine();
                var isBlank = string.IsNullOrWhiteSpace(input);

                if (textBuilder.Length == 0)
                {
                    if (isBlank)
                        break;
                    else if (input == "#showTree")
                    {
                        showTree = !showTree;
                        Console.WriteLine(showTree ? "Showing parse trees." : "Not showing parse trees");
                        continue;
                    }
                    else if (input == "#cls")
                    {
                        Console.Clear();
                        continue;
                    }
                }

                textBuilder.AppendLine(input);
                var text = textBuilder.ToString();

                var ast = AST.Parse(text);

                if (!isBlank && ast.Diagnostics.Any())
                    continue;

                var compilation = new Compilation(ast);
                var result      = compilation.Evaluate(variables);

                if (showTree)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    ast.Root.WriteTo(Console.Out);
                    Console.ForegroundColor = color;
                }

                if (!result.Diagnostics.Any())
                {
                    Console.WriteLine(result.Value);
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

                textBuilder.Clear();
            }
        }
    }
}
