namespace HyperLang.SimpleDemo;

sealed class NumberExpression : Expression
{
    public override SyntaxKind Kind       => SyntaxKind.NumberExpression;
    public          Token      NumberToken { get; }

    public NumberExpression(Token numberToken)
    {
        NumberToken = numberToken;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return NumberToken;
    }
}
