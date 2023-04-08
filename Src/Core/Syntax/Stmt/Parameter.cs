using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class Parameter : Node
{
    public Parameter(Token identifier, TypeClause type)
    {
        Identifier = identifier;
        Type = type;
    }

    public override SyntaxKind Kind       => SyntaxKind.Parameter;
    public          Token      Identifier { get; }
    public          TypeClause Type       { get; }
}
