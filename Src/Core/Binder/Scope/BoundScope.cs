using System.Collections.Immutable;
using Hyper.Core.Symbols;

namespace Hyper.Core.Binding.Scope;

internal sealed class BoundScope
{
    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
    }

    public bool TryDeclareVariable(VariableSymbol variable) => TryDeclareSymbol(variable);
    public bool TryDeclareFunction(FunctionSymbol function) => TryDeclareSymbol(function);

    public bool TryDeclareSymbol<TSymbol>(TSymbol symbol)
        where TSymbol : Symbol
    {
        _symbols ??= new Dictionary<string, Symbol>();

        if (_symbols.ContainsKey(symbol.Name))
            return false;

        _symbols.Add(symbol.Name, symbol);
        return true;
    }

    public Symbol? TryLookUpSymbol(string name)
    {
        if (_symbols != null && _symbols.TryGetValue(name, out var symbol))
            return symbol;

        return Parent?.TryLookUpSymbol(name);
    }

    public ImmutableArray<VariableSymbol> GetDeclaredVariables() => GetDeclaredSymbols<VariableSymbol>();
    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions() => GetDeclaredSymbols<FunctionSymbol>();

    private ImmutableArray<TSymbol> GetDeclaredSymbols<TSymbol>() where TSymbol : Symbol
    {
        return _symbols == null ? ImmutableArray<TSymbol>.Empty : _symbols.Values.OfType<TSymbol>().ToImmutableArray();
    }

    private Dictionary<string, Symbol>? _symbols;

    public BoundScope? Parent { get; }
}
