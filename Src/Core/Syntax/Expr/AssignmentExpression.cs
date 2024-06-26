﻿using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed class AssignmentExpression : Expression
{
    public AssignmentExpression(AST syntaxTree, Token identifierToken, Token equalsToken, Expression expression)
        : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
        EqualsToken = equalsToken;
        Expression = expression;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return IdentifierToken;
        yield return EqualsToken;
        yield return Expression;
    }

    public override SyntaxKind Kind            => SyntaxKind.AssignmentExpression;
    public          Token      IdentifierToken { get; }
    public          Token      EqualsToken     { get; }
    public          Expression Expression      { get; }
}
