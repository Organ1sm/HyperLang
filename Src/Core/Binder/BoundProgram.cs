using System.Collections.Immutable;
using Hyper.Core.Binding.Stmt;
using Hyper.Core.Symbols;

namespace Hyper.Core.Binding;

internal sealed class BoundProgram
{
    public BoundProgram(BoundProgram? previous,
                        BoundBlockStatement blockStatement,
                        ImmutableArray<Diagnostic.Diagnostic> diagnostics,
                        ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions)
    {
        Previous = previous;
        BlockStatement = blockStatement;
        Diagnostics = diagnostics;
        Functions = functions;
    }

    public BoundProgram?                                             Previous       { get; }
    public BoundBlockStatement                                      BlockStatement { get; }
    public ImmutableArray<Diagnostic.Diagnostic>                    Diagnostics    { get; }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions      { get; }
}
