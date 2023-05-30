﻿using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed partial class ParenthesizedExpression : Expression
{
    public override SyntaxKind Kind                  => SyntaxKind.ParenthesizedExpression;
    public          Token      OpenParenthesisToken  { get; }
    public          Expression Expression            { get; }
    public          Token      CloseParenthesisToken { get; }

    public ParenthesizedExpression(AST syntaxTree,
                                   Token openParenthesisToken,
                                   Expression expression,
                                   Token closeParenthesisToken)
        : base(syntaxTree)
    {
        OpenParenthesisToken = openParenthesisToken;
        Expression = expression;
        CloseParenthesisToken = closeParenthesisToken;
    }
}
