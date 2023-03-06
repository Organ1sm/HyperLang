﻿namespace HyperLang.SimpleDemo;

public sealed class LiteralExpression : Expression
{
    public override SyntaxKind Kind         => SyntaxKind.LiteralExpression;
    public          Token      LiteralToken { get; }

    public LiteralExpression(Token literalToken)
    {
        LiteralToken = literalToken;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return LiteralToken;
    }
}
