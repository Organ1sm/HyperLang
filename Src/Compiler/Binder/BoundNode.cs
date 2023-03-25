using System.Reflection;

namespace Hyper.Compiler.Binding;

internal abstract class BoundNode
{
    public abstract BoundNodeKind Kind { get; }

    public IEnumerable<BoundNode> GetChildren()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (typeof(BoundNode).IsAssignableFrom(property.PropertyType))
            {
                var child = (BoundNode) property.GetValue(this)!;
                yield return child;
            }
            else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
            {
                var children = (IEnumerable<BoundNode>) property.GetValue(this)!;
                foreach (var child in children)
                {
                    yield return child;
                }
            }
        }
    }

    private IEnumerable<(string Name, object Value)> GetProperties()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.Name == nameof(Kind) ||
                property.Name == nameof(BoundBinaryExpression.Operator))
                continue;

            if (typeof(BoundNode).IsAssignableFrom(property.PropertyType) ||
                typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                continue;

            var value = property.GetValue(this);
            if (value != null)
                yield return (property.Name, value);
        }
    }

    public void WriteTo(TextWriter writer)
    {
        PrettyPrint(writer, this);
    }

    private static void PrettyPrint(TextWriter writer, BoundNode node, string indent = "", bool isLast = true)
    {
        var isToConsole = writer == Console.Out;
        var marker      = isLast ? "└──" : "├──";

        if (isToConsole)
            Console.ForegroundColor = ConsoleColor.DarkGray;

        writer.Write(indent);
        writer.Write(marker);

        if (isToConsole)
            Console.ForegroundColor = GetColor(node);

        var text = GetText(node);
        writer.Write(text);

        var isFirstProperty = true;

        foreach (var p in node.GetProperties())
        {
            if (isFirstProperty)
                isFirstProperty = false;
            else
            {
                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.White;

                writer.Write(",");
            }

            writer.Write(" ");

            if (isToConsole)
                Console.ForegroundColor = ConsoleColor.Yellow;

            writer.Write(p.Name);

            if (isToConsole)
                Console.ForegroundColor = ConsoleColor.White;

            writer.Write(" = ");

            if (isToConsole)
                Console.ForegroundColor = ConsoleColor.DarkYellow;

            writer.Write(p.Value);
        }

        if (isToConsole)
            Console.ResetColor();

        writer.WriteLine();

        indent += isLast ? "   " : "│  ";

        var lastChild = node.GetChildren().LastOrDefault();

        foreach (var child in node.GetChildren())
            PrettyPrint(writer, child, indent, child == lastChild);
    }

    private static string GetText(BoundNode node)
    {
        return node switch
        {
            BoundBinaryExpression b => b.Operator?.Kind.ToString() + "Expression",
            BoundUnaryExpression u  => u.Operator.Kind.ToString() + "Expression",
            _                       => node.Kind.ToString()
        };
    }

    private static ConsoleColor GetColor(BoundNode node)
    {
        return node switch
        {
            BoundExpression => ConsoleColor.Blue,
            BoundStatement  => ConsoleColor.Cyan,
            _               => ConsoleColor.Yellow
        };
    }

    public override string ToString()
    {
        using (var writer = new StringWriter())
        {
            WriteTo(writer);
            return writer.ToString();
        }
    }
}
