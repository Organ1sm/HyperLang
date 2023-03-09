using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax;

public sealed class LiteralExpression : Expression
{
    public override SyntaxKind Kind         => SyntaxKind.LiteralExpression;
    public          Token      LiteralToken { get; }
    public          object     Value        { get; }

    public LiteralExpression(Token literalToken)
        : this(literalToken, literalToken.Value) { }

    public LiteralExpression(Token literalToken, object value)
    {
        LiteralToken = literalToken;
        Value = value;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return LiteralToken;
    }
}
