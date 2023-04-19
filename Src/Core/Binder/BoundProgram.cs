using System.Collections.Immutable;
using Hyper.Core.Binding.Stmt;
using Hyper.Core.Symbols;

namespace Hyper.Core.Binding;

internal sealed class BoundProgram
{
    public BoundProgram(BoundBlockStatement blockStatement,
                        ImmutableArray<Diagnostic.Diagnostic> diagnostics,
                        ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions)
    {
        BlockStatement = blockStatement;
        Diagnostics = diagnostics;
        Functions = functions;
    }

    public BoundBlockStatement                                      BlockStatement  { get; }
    public ImmutableArray<Diagnostic.Diagnostic>                    Diagnostics { get; }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions   { get; }
}
