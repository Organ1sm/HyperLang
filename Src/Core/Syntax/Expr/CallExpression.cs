using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public class CallExpression : Expression
{
    public CallExpression(AST syntaxTree,Token identifier,
                          Token openParenthesisToken,
                          SeparatedSyntaxList<Expression> arguments,
                          Token closeParenthesisToken)
    : base(syntaxTree)
    {
        Identifier = identifier;
        OpenParenthesisToken = openParenthesisToken;
        Arguments = arguments;
        CloseParenthesisToken = closeParenthesisToken;
    }

    public override SyntaxKind                      Kind                  => SyntaxKind.CallExpression;
    public          Token                           Identifier            { get; }
    public          Token                           OpenParenthesisToken  { get; }
    public          SeparatedSyntaxList<Expression> Arguments             { get; }
    public          Token                           CloseParenthesisToken { get; }
}
