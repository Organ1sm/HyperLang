using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed class LiteralExpression : Expression
{
    public override SyntaxKind Kind         => SyntaxKind.LiteralExpression;
    public          Token      LiteralToken { get; }
    public          object     Value        { get; }

    public LiteralExpression(AST syntaxTree, Token literalToken)
        : this(syntaxTree, literalToken, literalToken.Value!) { }

    public LiteralExpression(AST syntaxTree, Token literalToken, object value)
        : base(syntaxTree)
    {
        LiteralToken = literalToken;
        Value = value;
    }
}
