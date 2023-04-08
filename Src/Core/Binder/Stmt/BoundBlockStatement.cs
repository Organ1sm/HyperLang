using System.Collections.Immutable;
using Hyper.Core.Binding.Expr;

namespace Hyper.Core.Binding.Stmt;

internal sealed class BoundBlockStatement : BoundStatement
{
    public BoundBlockStatement(ImmutableArray<BoundStatement?> statements)
    {
        Statements = statements;
    }

    public override BoundNodeKind                   Kind       => BoundNodeKind.BlockStatement;
    public          ImmutableArray<BoundStatement?> Statements { get; }
}
