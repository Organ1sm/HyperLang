using System.Collections.Immutable;
using Hyper.Compiler.Symbols;

namespace Hyper.Compiler.Binding;

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

    public bool TryLookUpVariable(string name, out VariableSymbol? variable) => TryLookUpSymbol(name, out variable);
    public bool TryLookUpFunction(string name, out FunctionSymbol? function) => TryLookUpSymbol(name, out function);

    private bool TryLookUpSymbol<TSymbol>(string name, out TSymbol? symbol)
        where TSymbol : Symbol
    {
        symbol = null;

        if (_symbols != null && _symbols.TryGetValue(name, out var declaredSymbol))
        {
            if (declaredSymbol is TSymbol matchingSymbol)
            {
                symbol = matchingSymbol;
                return true;
            }

            return false;
        }

        if (Parent == null)
            return false;

        return Parent.TryLookUpSymbol(name, out symbol);
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
