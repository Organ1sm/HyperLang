using System.Collections.Immutable;
using Hyper.Core.Binding.Stmt;
using Hyper.Core.Symbols;

namespace Hyper.Core.Binding;

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
