using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed partial class VariableDeclaration : Statement
{
    public VariableDeclaration(AST syntaxTree,
                               Token keyword,
                               Token identifier,
                               TypeClause? typeClause,
                               Token equalsToken,
                               Expression initializer)
        : base(syntaxTree)
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
