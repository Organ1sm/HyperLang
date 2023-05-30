using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed partial class Parameter : Node
{
    public Parameter(AST syntaxTree, Token identifier, TypeClause type)
        : base(syntaxTree)
    {
        Identifier = identifier;
        Type = type;
    }

    public override SyntaxKind Kind       => SyntaxKind.Parameter;
    public          Token      Identifier { get; }
    public          TypeClause Type       { get; }
}
