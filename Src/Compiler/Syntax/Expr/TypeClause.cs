using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax;

public sealed class TypeClause : Node
{
    public TypeClause(Token colonToken, Token identifier)
    {
        ColonToken = colonToken;
        Identifier = identifier;
    }

    public override SyntaxKind Kind       => SyntaxKind.TypeClause;
    public          Token      ColonToken { get; }
    public          Token      Identifier { get; }
}
