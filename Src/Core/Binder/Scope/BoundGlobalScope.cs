using System.Collections.Immutable;
using Hyper.Core.Binding.Stmt;
using Hyper.Core.Symbols;

namespace Hyper.Core.Binding.Scope;

internal sealed class BoundGlobalScope

{
    public BoundGlobalScope(BoundGlobalScope? previous,
                            ImmutableArray<Diagnostic.Diagnostic> diagnostics,
                            FunctionSymbol? mainFunction,
                            FunctionSymbol? scriptFunction,
                            ImmutableArray<VariableSymbol> variables,
                            ImmutableArray<FunctionSymbol> functions,
                            ImmutableArray<BoundStatement> statements)

    {
        Previous = previous;
        MainFunction = mainFunction;
        ScriptFunction = scriptFunction;
        Diagnostics = diagnostics;
        Variables = variables;
        Functions = functions;
        Statements = statements;
    }

    public BoundGlobalScope?                     Previous     { get; }
    public ImmutableArray<Diagnostic.Diagnostic> Diagnostics  { get; }
    public FunctionSymbol?                       MainFunction { get; }
    public FunctionSymbol?                        ScriptFunction { get; }
    public ImmutableArray<VariableSymbol>        Variables      { get; }
    public ImmutableArray<FunctionSymbol>        Functions      { get; }
    public ImmutableArray<BoundStatement>        Statements     { get; }
}
