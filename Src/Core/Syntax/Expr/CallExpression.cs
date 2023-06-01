using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed class CallExpression : Expression
{
    public CallExpression(AST syntaxTree,
                          Token identifier,
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

    public override SyntaxKind Kind => SyntaxKind.CallExpression;

    public override IEnumerable<Node> GetChildren()
    {
        yield return Identifier;
        yield return OpenParenthesisToken;

        foreach (var child in Arguments.GetWithSeparators())
            yield return child;

        yield return CloseParenthesisToken;
    }

    public Token                           Identifier            { get; }
    public Token                           OpenParenthesisToken  { get; }
    public SeparatedSyntaxList<Expression> Arguments             { get; }
    public Token                           CloseParenthesisToken { get; }
}
