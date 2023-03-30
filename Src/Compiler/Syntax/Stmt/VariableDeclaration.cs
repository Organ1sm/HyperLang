using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax.Stmt;

public sealed class VariableDeclaration : Statement
{
    public VariableDeclaration(Token keyword,
                               Token identifier,
                               TypeClause? typeClause,
                               Token equalsToken,
                               Expression initializer)
    {
        Keyword = keyword;
        Identifier = identifier;
        TypeClause = typeClause;
        EqualsToken = equalsToken;
        Initializer = initializer;
    }

    public override SyntaxKind  Kind        => SyntaxKind.VariableDeclaration;
    public          Token       Keyword     { get; }
    public          Token       Identifier  { get; }
    public          TypeClause? TypeClause  { get; }
    public          Token       EqualsToken { get; }
    public          Expression  Initializer { get; }
}
