using System.Reflection;
using Hyper.Core.Parser;
using Hyper.Core.Syntax;
using Hyper.Core.Syntax.Expr;
using Xunit;

namespace Hyper.Test.Compiler.Syntax;

internal sealed class AssertingEnumerator : IDisposable
{
    private readonly IEnumerator<Node> _enumerator;
    private          bool              _hasErrors;

    public AssertingEnumerator(Node node)
    {
        _enumerator = Flatten(node).GetEnumerator();
    }

    private bool MarkFailed()
    {
        _hasErrors = true;
        return false;
    }

    public void Dispose()
    {
        if (!_hasErrors)
            Assert.False(_enumerator.MoveNext());

        _enumerator.Dispose();
    }

    private static IEnumerable<Node> Flatten(Node node)
    {
        var stack = new Stack<Node>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var n = stack.Pop();
            yield return n;

            foreach (var child in n.GetChildren().Reverse())
                stack.Push(child);
        }
    }

    public void AssertNode(SyntaxKind kind)
    {
        try
        {
            Assert.True(_enumerator.MoveNext());
            Assert.Equal(kind, _enumerator.Current.Kind);
            Assert.IsNotType<Node>(_enumerator.Current);
        }
        catch when (MarkFailed())
        {
            throw;
        }
    }

    public void AssertToken(SyntaxKind kind, string text)
    {
        try
        {
            Assert.True(_enumerator.MoveNext());
            Assert.Equal(kind, _enumerator.Current.Kind);

            var token = Assert.IsType<Token>(_enumerator.Current);
            Assert.Equal(text, token.Text);
        }
        catch when (MarkFailed())
        {
            throw;
        }
    }
}
