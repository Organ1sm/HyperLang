using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed class TypeClause : Node
{
    public TypeClause(Token colonOrArrowToken, Token identifier)
    {
        ColonOrArrowToken = colonOrArrowToken;
        Identifier = identifier;
    }

    public override SyntaxKind Kind              => SyntaxKind.TypeClause;
    public          Token      ColonOrArrowToken { get; }
    public          Token      Identifier        { get; }
}
