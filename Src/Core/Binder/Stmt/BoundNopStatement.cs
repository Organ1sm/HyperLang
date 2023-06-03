using Hyper.Core.Binding.Expr;

namespace Hyper.Core.Binding.Stmt;

internal sealed class BoundNopStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.NopStatement;
}
