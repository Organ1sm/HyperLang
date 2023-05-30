using Hyper.Core.Syntax;
using Hyper.Core.Syntax.Expr;
using Hyper.Core.Text;

namespace Hyper.Core.Parser
{
    public sealed class Token : Node
    {
        public Token(AST syntaxTree, SyntaxKind kind, int position, string? text = null, object? value = null)
            : base(syntaxTree)
        {
            Kind = kind;
            Position = position;
            Text = text ?? string.Empty;
            IsMissing = text == null;
            Value = value;
        }

        public override IEnumerable<Node> GetChildren() => Array.Empty<Node>();

        public override SyntaxKind Kind { get; }

        public          int      Position { get; }
        public          string   Text     { get; }
        public          object?  Value    { get; }
        public override TextSpan Span     => new(Position, Text.Length);

        /// <summary>
        /// A token is missing if it was inserted by the parser and doesn't appear in source.
        /// </summary>
        public bool IsMissing { get; }
    }
}