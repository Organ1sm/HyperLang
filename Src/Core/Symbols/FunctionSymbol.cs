using System.Collections.Immutable;
using Hyper.Core.Syntax.Stmt;

namespace Hyper.Core.Symbols;

public class FunctionSymbol : Symbol
{
    public FunctionSymbol(string name,
                          ImmutableArray<ParameterSymbol> parameters,
                          TypeSymbol type,
                          FunctionDeclaration? declaration = null)
        : base(name)
    {
        Parameters = parameters;
        Type = type;
        Declaration = declaration;
    }

    public override SymbolKind                      Kind        => SymbolKind.Function;
    public          FunctionDeclaration?            Declaration { get; }
    public          ImmutableArray<ParameterSymbol> Parameters  { get; }
    public          TypeSymbol                      Type        { get; }
}
