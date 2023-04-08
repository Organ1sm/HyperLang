using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed class LiteralExpression : Expression
{
    public override SyntaxKind Kind         => SyntaxKind.LiteralExpression;
    public          Token      LiteralToken { get; }
    public          object?    Value        { get; }

    public LiteralExpression(Token literalToken)
        : this(literalToken, literalToken.Value) { }

    public LiteralExpression(Token literalToken, object? value)
    {
        LiteralToken = literalToken;
        Value = value;
    }
}
