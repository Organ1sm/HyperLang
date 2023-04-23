using Hyper.Core.Symbols;
using Hyper.Core.Syntax;
using Hyper.Core.VM;
using Hyper.Core.IO;

namespace HyperC;

internal static class Program

{
    private static void Main(string[] args)
    {
        switch (args.Length)
        {
            case 0:
                Console.Error.WriteLine("usage: mc <source-paths>");
                return;
            case > 1:
                Console.WriteLine("error: only one path supported right now.");
                return;
        }

        var path = args.Single();
        if (!File.Exists(path))
        {
            Console.WriteLine($"error: file '{path}' doesn't exist");
            return;
        }

        var syntaxTree = AST.Load(path);

        var compilation = new Compilation(syntaxTree);
        var result      = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        if (!result.Diagnostics.Any())
        {
            if (result.Value != null)
                Console.WriteLine(result.Value);
        }
        else
        {
            Console.Error.WriteDiagnostics(result.Diagnostics);
        }
    }
}
