using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Opt;
using Hyper.Core.Binding.Stmt;

namespace Hyper.Core.Binding.Opt;

internal sealed class BasicBlockBuilder
{
    private List<BoundStatement> _statements = new();
    private List<BasicBlock>     _blocks     = new();

    public List<BasicBlock> Build(BoundBlockStatement blockStatement)
    {
        foreach (var statement in blockStatement.Statements.Where(statement => statement != null))
        {
            switch (statement.Kind)
            {
                case BoundNodeKind.LabelStatement:
                    StartBlock();
                    _statements.Add(statement);
                    break;

                case BoundNodeKind.GotoStatement:
                case BoundNodeKind.ConditionalGotoStatement:
                case BoundNodeKind.ReturnStatement:
                    _statements.Add(statement);
                    StartBlock();
                    break;

                case BoundNodeKind.VariableDeclaration:
                case BoundNodeKind.ExpressionStatement:
                    _statements.Add(statement);
                    break;
                default:
                    throw new Exception($"Unexpected statement: {statement.Kind}");
            }
        }

        EndBlock();

        return _blocks.ToList();
    }

    private void StartBlock() => EndBlock();

    private void EndBlock()
    {
        if (_statements.Count <= 0) return;

        var block = new BasicBlock();
        block.Statements.AddRange(_statements);
        _blocks.Add(block);
        _statements.Clear();
    }
}
