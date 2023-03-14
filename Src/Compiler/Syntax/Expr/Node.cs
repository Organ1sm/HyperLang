using System.Reflection;

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
}
