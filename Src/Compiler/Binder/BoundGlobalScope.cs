using System.Collections.Immutable;
using Hyper.Compiler.Symbol;

namespace Hyper.Compiler.Binding;

internal sealed class BoundGlobalScope

{
    public BoundGlobalScope(BoundGlobalScope previous,
                            ImmutableArray<Diagnostic.Diagnostic> diagnostics,
                            ImmutableArray<VariableSymbol> variables,
                            BoundExpression expression)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        Variables = variables;
        Expression = expression;
    }

    public BoundGlobalScope                      Previous    { get; }
    public ImmutableArray<Diagnostic.Diagnostic> Diagnostics { get; }
    public ImmutableArray<VariableSymbol>        Variables   { get; }
    public BoundExpression                       Expression  { get; }
}
