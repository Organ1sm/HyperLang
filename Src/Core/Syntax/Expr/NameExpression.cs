using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed class NameExpression : Expression
{
    public NameExpression(Token identifierToken)
    {
        IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind            => SyntaxKind.NameExpression;
    public          Token      IdentifierToken { get; }
}
