using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed class AssignmentExpression : Expression
{
    public AssignmentExpression(Token identifierToken, Token equalsToken, Expression expression)
    {
        IdentifierToken = identifierToken;
        EqualsToken = equalsToken;
        Expression = expression;
    }

    public override SyntaxKind Kind            => SyntaxKind.AssignmentExpression;
    public          Token      IdentifierToken { get; }
    public          Token      EqualsToken     { get; }
    public          Expression Expression      { get; }
}
