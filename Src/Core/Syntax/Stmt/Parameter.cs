using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class Parameter : Node
{
    public Parameter(AST syntaxTree, Token identifier, TypeClause type)
        : base(syntaxTree)
    {
        Identifier = identifier;
        Type = type;
    }

    public override SyntaxKind Kind => SyntaxKind.Parameter;

    public override IEnumerable<Node> GetChildren()
    {
        yield return Identifier;
        yield return Type;
    }

    public Token      Identifier { get; }
    public TypeClause Type       { get; }
}
