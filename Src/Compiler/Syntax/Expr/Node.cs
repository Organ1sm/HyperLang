using System.Reflection;
using Hyper.Compiler.Parser;
using Hyper.Compiler.Text;

namespace Hyper.Compiler.Syntax;

public class Node
{
    public virtual SyntaxKind Kind { get; }

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

    public void WriteTo(TextWriter writer)
    {
        PrettyPrint(writer, this);
    }

    private static void PrettyPrint(TextWriter writer, Node node, string indent = "", bool isLast = true)
    {
        var marker      = isLast ? "└──" : "├──";
        var isToConsole = (writer == Console.Out);

        if (isToConsole)
            Console.ForegroundColor = ConsoleColor.DarkGray;

        writer.Write(indent);
        writer.Write(marker);

        if (isToConsole)
            Console.ForegroundColor = node is Token ? ConsoleColor.Blue : ConsoleColor.Cyan;

        writer.Write(node.Kind);

        if (node is Token t && t.Value != null)
        {
            writer.Write(" ");
            writer.Write(t.Value);
        }

        if (isToConsole)
            Console.ResetColor();

        writer.WriteLine();

        indent += isLast ? "    " : "│  ";

        var lastChild = node.GetChildren().LastOrDefault();

        foreach (var child in node.GetChildren())
            PrettyPrint(writer, child, indent, child == lastChild);
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
