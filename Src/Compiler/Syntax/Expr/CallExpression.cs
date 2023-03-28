﻿using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax;

public class CallExpression : Expression
{
    public CallExpression(Token identifier,
                          Token openParenthesisToken,
                          SeparatedSyntaxList<Expression> arguments,
                          Token closeParenthesisToken)
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