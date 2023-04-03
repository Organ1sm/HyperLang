using System.Collections.Immutable;
using Hyper.Compiler.Symbols;

namespace Hyper.Compiler.Binding;

internal sealed class BoundGlobalScope

{
    public BoundGlobalScope(BoundGlobalScope? previous,
                            ImmutableArray<Diagnostic.Diagnostic> diagnostics,
                            ImmutableArray<VariableSymbol>? variables,
                            ImmutableArray<FunctionSymbol>? functions,
                            ImmutableArray<BoundStatement> statements)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        Variables = variables;
        Functions = functions;
        Statements = statements;
    }

    public BoundGlobalScope?                     Previous    { get; }
    public ImmutableArray<Diagnostic.Diagnostic> Diagnostics { get; }
    public ImmutableArray<VariableSymbol>?       Variables   { get; }
    public ImmutableArray<FunctionSymbol>?       Functions   { get; }
    public ImmutableArray<BoundStatement>        Statements  { get; }
}
