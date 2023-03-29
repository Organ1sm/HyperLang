using System.Collections;
using System.Collections.Immutable;
using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax;

public abstract class SeparatedSyntaxList
{
    public abstract ImmutableArray<Node> GetWithSeparators();
}

public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList, IEnumerable<T>
    where T : Node
{
    public SeparatedSyntaxList(ImmutableArray<Node> nodesAndSeparators)
    {
        _nodesAndSeparators = nodesAndSeparators;
    }

    private readonly ImmutableArray<Node> _nodesAndSeparators;

    public int Count => (_nodesAndSeparators.Length + 1) / 2;

    /// <summary>
    /// return the 2 * index parameter. 
    /// </summary>
    /// <param name="index"></param>
    public T this[int index] => (T) _nodesAndSeparators[index * 2];

    public Token? GetSeparator(int index)
    {
        if (index == Count - 1)
            return null;

        return (Token) _nodesAndSeparators[index * 2 + 1];
    }

    public override ImmutableArray<Node> GetWithSeparators() => _nodesAndSeparators;

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
