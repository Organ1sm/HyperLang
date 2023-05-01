using System.Collections.Immutable;
using Hyper.Core.Binding.Stmt;
using Hyper.Core.Symbols;

namespace Hyper.Core.Binding;

internal sealed class BoundProgram
{
    public BoundProgram(BoundProgram? previous,
                        ImmutableArray<Diagnostic.Diagnostic> diagnostics,
                        FunctionSymbol? mainFunction,
                        FunctionSymbol? scriptFunction,
                        ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        MainFunction = mainFunction;
        ScriptFunction = scriptFunction;
        Functions = functions;
    }

    public BoundProgram?                                            Previous       { get; }
    public ImmutableArray<Diagnostic.Diagnostic>                    Diagnostics    { get; }
    public FunctionSymbol?                                          MainFunction   { get; }
    public FunctionSymbol?                                          ScriptFunction { get; }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions      { get; }
}
