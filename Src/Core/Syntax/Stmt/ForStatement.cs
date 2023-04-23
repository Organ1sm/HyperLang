using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class ForStatement : Statement
{
    // for <var> = <lower> to <upper>
    // <body>
    //  ....
    public ForStatement(AST syntaxTree,
                        Token keyword,
                        Token identifier,
                        Token equalsToken,
                        Expression lowerBound,
                        Token toKeyword,
                        Expression upperBound,
                        Statement body)
        : base(syntaxTree)
    {
        Keyword = keyword;
        Identifier = identifier;
        EqualsToken = equalsToken;
        LowerBound = lowerBound;
        ToKeyword = toKeyword;
        UpperBound = upperBound;
        Body = body;
    }

    public override SyntaxKind Kind        => SyntaxKind.ForStatement;
    public          Token      Keyword     { get; }
    public          Token      Identifier  { get; }
    public          Token      EqualsToken { get; }
    public          Expression LowerBound  { get; }
    public          Token      ToKeyword   { get; }
    public          Expression UpperBound  { get; }
    public          Statement  Body        { get; }
}
