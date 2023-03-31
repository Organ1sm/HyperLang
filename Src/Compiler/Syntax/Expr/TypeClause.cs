using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax;

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
