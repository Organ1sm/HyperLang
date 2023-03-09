using Hyper.Compiler.Syntax;
using Hyper.Compiler.Text;

namespace Hyper.Compiler.Parser
{
    public sealed class Token : Node
    {
        public Token(SyntaxKind kind, int position, string? text = null, object? value = null)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }

        public override SyntaxKind Kind { get; }

        public int      Position { get; }
        public string   Text     { get; }
        public object?  Value    { get; }
        public TextSpan Span     => new(Position, Text.Length);

        public override IEnumerable<Node> GetChildren()
        {
            return Enumerable.Empty<Node>();
        }
    }
}
