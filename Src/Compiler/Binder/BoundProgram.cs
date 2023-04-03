using System.Collections.Immutable;
using Hyper.Compiler.Diagnostic;
using Hyper.Compiler.Symbols;

namespace Hyper.Compiler.Binding;

internal sealed class BoundProgram
{
    public BoundProgram(BoundBlockStatement statements,
                        ImmutableArray<Diagnostic.Diagnostic> diagnostics,
                        ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions)
    {
        Statements = statements;
        Diagnostics = diagnostics;
        Functions = functions;
    }

    public BoundBlockStatement                                      Statements  { get; }
    public ImmutableArray<Diagnostic.Diagnostic>                    Diagnostics { get; }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions   { get; }
}
