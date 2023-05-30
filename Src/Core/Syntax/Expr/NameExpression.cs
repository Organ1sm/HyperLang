using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed partial class NameExpression : Expression
{
    public NameExpression(AST syntaxTree, Token identifierToken)
        : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind            => SyntaxKind.NameExpression;
    public          Token      IdentifierToken { get; }
}
