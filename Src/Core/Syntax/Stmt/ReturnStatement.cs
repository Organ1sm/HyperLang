using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed partial class ReturnStatement : Statement
{
    public ReturnStatement(AST syntaxTree, Token returnKeyword, Expression? expression)
        : base(syntaxTree)
    {
        ReturnKeyword = returnKeyword;
        Expression = expression;
    }

    public override SyntaxKind  Kind          => SyntaxKind.ReturnStatement;
    public          Token       ReturnKeyword { get; }
    public          Expression? Expression    { get; }
}
