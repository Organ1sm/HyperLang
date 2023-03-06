namespace HyperLang.SimpleDemo;

abstract class Node
{
    public abstract SyntaxKind Kind { get; }

    public abstract IEnumerable<Node> GetChildren();
}
