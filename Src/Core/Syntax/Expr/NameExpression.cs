using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed class NameExpression : Expression
{
    public NameExpression(AST syntaxTree, Token identifierToken)
        : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind            => SyntaxKind.NameExpression;
    public override IEnumerable<Node> GetChildren()
    {
        yield return IdentifierToken;
    }

    public          Token      IdentifierToken { get; }
}
