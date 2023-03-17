using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax.Stmt;

public sealed class VariableDeclaration : Statement
{
    public VariableDeclaration(Token keyword, Token identifier, Token equalsToken, Expression initializer)
    {
        Keyword = keyword;
        Identifier = identifier;
        EqualsToken = equalsToken;
        Initializer = initializer;
    }

    public override SyntaxKind Kind        => SyntaxKind.VariableDeclaration;
    public          Token      Keyword     { get; }
    public          Token      Identifier  { get; }
    public          Token      EqualsToken { get; }
    public          Expression Initializer { get; }
}
