using System.Collections.Immutable;
using Hyper.Compiler.Symbols;

namespace Hyper.Compiler.Binding;

internal sealed class BoundScope
{
    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
    }

    public bool TryDeclareVariable(VariableSymbol variable)
    {
        _variables ??= new Dictionary<string, VariableSymbol>();

        if (_variables.ContainsKey(variable.Name))
            return false;

        _variables.Add(variable.Name, variable);
        return true;
    }

    public bool TryLookUpVariable(string name, out VariableSymbol? variable)
    {
        variable = null;
        
        if (_variables != null && _variables.TryGetValue(name, out variable))
            return true;

        if (Parent == null)
            return false;

        return Parent.TryLookUpVariable(name, out variable);
    }

    public bool TryDeclareFunction(FunctionSymbol? function)
    {
        _functions ??= new Dictionary<string, FunctionSymbol>();

        if (_functions.ContainsKey(function.Name))
            return false;

        _functions.Add(function.Name, function);
        return true;
    }

    public bool TryLookupFunction(string? name, out FunctionSymbol function)
    {
        function = null;

        if (_functions != null && _functions.TryGetValue(name, out function))
            return true;

        if (Parent == null)
            return false;

        return Parent.TryLookupFunction(name, out function);
    }

    public ImmutableArray<VariableSymbol> GetDeclaredVariables()
    {
        return _variables == null ? ImmutableArray<VariableSymbol>.Empty : _variables.Values.ToImmutableArray();
    }

    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
    {
        return _functions == null ? ImmutableArray<FunctionSymbol>.Empty : _functions.Values.ToImmutableArray();
    }

    private Dictionary<string, VariableSymbol>? _variables;
    private Dictionary<string, FunctionSymbol>? _functions;

    public BoundScope? Parent { get; }
}
