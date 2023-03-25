using System.Collections.Immutable;
using Hyper.Compiler.Symbols;

namespace Hyper.Compiler.Binding;

internal sealed class BoundScope
{
    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
    }

    public bool TryDeclare(VariableSymbol variable)
    {
        if (_variables.ContainsKey(variable.Name))
            return false;

        _variables.Add(variable.Name, variable);
        return true;
    }

    public bool TryLookUp(string name, out VariableSymbol variable)
    {
        if (_variables.TryGetValue(name, out variable))
            return true;

        if (Parent == null)
            return false;

        return Parent.TryLookUp(name, out variable);
    }

    public ImmutableArray<VariableSymbol> GetDeclaredVariables()
    {
        return _variables.Values.ToImmutableArray();
    }

    private Dictionary<string, VariableSymbol> _variables = new();

    public BoundScope? Parent { get; }
}
