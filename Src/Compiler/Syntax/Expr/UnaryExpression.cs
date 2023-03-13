﻿using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax;

public sealed class UnaryExpression : Expression
{
    public override SyntaxKind Kind     => SyntaxKind.UnaryExpression;
    public          Token      Operator { get; }
    public          Expression Operand  { get; }

    public UnaryExpression(Token @operator, Expression operand)
    {
        Operator = @operator;
        Operand = operand;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return Operator;
        yield return Operand;
    }
}