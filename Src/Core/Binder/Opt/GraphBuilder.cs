using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Operator;
using Hyper.Core.Binding.Stmt;
using Hyper.Core.Symbols;
using Hyper.Core.Syntax;

namespace Hyper.Core.Binding.Opt;

internal sealed class GraphBuilder
{
    private Dictionary<BoundStatement, BasicBlock> _blockFromStatement = new();
    private Dictionary<BoundLabel, BasicBlock>     _blockFromLabel     = new();
    private List<BasicBlockBranch>                 _branches           = new();

    private BasicBlock _start = new(isStart: true);
    private BasicBlock _end   = new(isStart: false);

    public ControlFlowGraph Build(List<BasicBlock> blocks)
    {
        Connect(_start, !blocks.Any() ? _end : blocks.First());

        foreach (var block in blocks)
        {
            foreach (var statement in block.Statements)
            {
                _blockFromStatement.Add(statement, block);
                if (statement is BoundLabelStatement labelStatement)
                    _blockFromLabel.Add(labelStatement.Label, block);
            }
        }

        for (int i = 0; i < blocks.Count; i++)
        {
            var current = blocks[i];
            var next    = i == blocks.Count - 1 ? _end : blocks[i + 1];

            foreach (var statement in current.Statements)
            {
                var isLastStatementInBlock = (statement == current.Statements.Last());
                switch (statement.Kind)
                {
                    case BoundNodeKind.GotoStatement:
                        var gs      = (BoundGotoStatement) statement;
                        var toBlock = _blockFromLabel[gs.Label];
                        Connect(current, toBlock);
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                    {
                        var cgs       = (BoundConditionalGotoStatement) statement;
                        var thenBlock = _blockFromLabel[cgs.Label];
                        var elseBlock = next;

                        var negatedCondition = Negate(cgs.Condition);
                        var thenCondition    = cgs.JumpIfTrue ? cgs.Condition : negatedCondition;
                        var elseCondition    = cgs.JumpIfTrue ? negatedCondition : cgs.Condition;

                        Connect(current, thenBlock, thenCondition);
                        Connect(current, elseBlock, elseCondition);

                        break;
                    }
                    case BoundNodeKind.ReturnStatement:
                        Connect(current, _end);
                        break;
                    case BoundNodeKind.VariableDeclaration:
                    case BoundNodeKind.LabelStatement:
                    case BoundNodeKind.ExpressionStatement:
                        if (isLastStatementInBlock)
                            Connect(current, next);
                        break;
                    default:
                        throw new Exception($"Unexpected statement: {statement.Kind}");
                }
            }
        }

        ScanAgain:
        foreach (var block in blocks.Where(block => !block.Incoming.Any()))
        {
            RemoveBlock(blocks, block);
            goto ScanAgain;
        }

        blocks.Insert(0, _start);
        blocks.Add(_end);

        return new ControlFlowGraph(_start, _end, blocks, _branches);
    }

    private void Connect(BasicBlock from, BasicBlock to, BoundExpression? condition = null)
    {
        if (condition is BoundLiteralExpression l)
        {
            var value = (bool) (l.Value ?? false);
            if (value)
                condition = null;
            else
                return;
        }

        var branch = new BasicBlockBranch(from, to, condition);
        from.Outgoing.Add(branch);
        to.Incoming.Add(branch);
        _branches.Add(branch);
    }

    private void RemoveBlock(List<BasicBlock> blocks, BasicBlock block)
    {
        foreach (var branch in block.Incoming)
        {
            branch.From.Outgoing.Remove(branch);
            _branches.Remove(branch);
        }

        foreach (var branch in block.Outgoing)
        {
            branch.To.Incoming.Remove(branch);
            _branches.Remove(branch);
        }

        blocks.Remove(block);
    }

    private BoundExpression Negate(BoundExpression condition)
    {
        if (condition is BoundLiteralExpression l)
        {
            var value = (bool) (l.Value ?? false);
            return new BoundLiteralExpression(!value);
        }

        var op = BoundUnaryOperator.Bind(SyntaxKind.BangToken, TypeSymbol.Bool);
        return new BoundUnaryExpression(op, condition);
    }
}
