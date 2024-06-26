﻿using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed class BinaryExpression : Expression
{
    public override SyntaxKind Kind     => SyntaxKind.BinaryExpression;
    public          Expression Left     { get; }
    public          Token      Operator { get; }
    public          Expression Right    { get; }

    public BinaryExpression(AST syntaxTree, Expression left, Token @operator, Expression right)
        : base(syntaxTree)
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
