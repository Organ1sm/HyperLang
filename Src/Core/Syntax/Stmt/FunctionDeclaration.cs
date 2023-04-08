using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class FunctionDeclaration : MemberSyntax
{
    public FunctionDeclaration(Token functionKeyword,
                               Token identifier,
                               Token openParenthesisToken,
                               SeparatedSyntaxList<Parameter> parameters,
                               Token closeParenthesisToken,
                               TypeClause? type,
                               BlockStatement body)
    {
        FunctionKeyword = functionKeyword;
        Identifier = identifier;
        OpenParenthesisToken = openParenthesisToken;
        Parameters = parameters;
        CloseParenthesisToken = closeParenthesisToken;
        Type = type;
        Body = body;
    }

    public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;

    public Token                          FunctionKeyword       { get; }
    public Token                          Identifier            { get; }
    public Token                          OpenParenthesisToken  { get; }
    public SeparatedSyntaxList<Parameter> Parameters            { get; }
    public Token                          CloseParenthesisToken { get; }
    public TypeClause?                     Type                  { get; }
    public BlockStatement                 Body                  { get; }
}
