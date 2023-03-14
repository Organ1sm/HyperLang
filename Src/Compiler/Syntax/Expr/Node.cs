using System.Reflection;
using Hyper.Compiler.Text;

namespace Hyper.Compiler.Syntax;

public abstract class Node
{
    public abstract SyntaxKind Kind { get; }

    public IEnumerable<Node> GetChildren()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (typeof(Node).IsAssignableFrom(property.PropertyType))
            {
                var child = (Node) property.GetValue(this);
                yield return child;
            }
            else if (typeof(IEnumerable<Node>).IsAssignableFrom(property.PropertyType))
            {
                var children = (IEnumerable<Node>) property.GetValue(this);
                foreach (var child in children)
                    yield return child;
            }
        }
    }

    public virtual TextSpan Span
    {
        get
        {
            var first = GetChildren().First().Span;
            var last  = GetChildren().Last().Span;

            return TextSpan.MakeTextSpanFromBound(first.Start, last.End);
        }
    }
}
