using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Expr;

public sealed partial class TypeClause : Node
{
    public TypeClause(AST syntaxTree, Token colonOrArrowToken, Token identifier)
        : base(syntaxTree)
    {
        ColonOrArrowToken = colonOrArrowToken;
        Identifier = identifier;
    }

    public override SyntaxKind Kind              => SyntaxKind.TypeClause;
    public          Token      ColonOrArrowToken { get; }
    public          Token      Identifier        { get; }
}
