﻿using System.Linq.Expressions;

namespace HyperLang.SimpleDemo;

public sealed class BinaryExpression : Expression
{
    public override SyntaxKind Kind     => SyntaxKind.BinaryExpression;
    public          Expression Left     { get; }
    public          Token      Operator { get; }
    public          Expression Right    { get; }

    public BinaryExpression(Expression left, Token @operator, Expression right)
    {
        Left = left;
        Operator = @operator;
        Right = right;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Left;
        yield return Operator;
        yield return Right;
    }
}
