using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public abstract class MemberSyntax : Node
{
    protected MemberSyntax(AST syntaxTree) : base(syntaxTree) { }
}
