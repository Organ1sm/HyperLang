﻿using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Stmt;

namespace Hyper.Core.Binding.Opt;

internal sealed class ControlFlowGraph
{
    public ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks, List<BasicBlockBranch> branches)
    {
        Start = start;
        End = end;
        Blocks = blocks;
        Branches = branches;
    }

    // Gets the start basic block.
    public BasicBlock Start { get; }

    // Gets the end basic block. 
    public BasicBlock End { get; }

    // All basic blocks contained in the CFG.
    public List<BasicBlock> Blocks { get; }

    // All basic blocks branches contained in the CFG.
    public List<BasicBlockBranch> Branches { get; }

    public void WriteTo(TextWriter writer)
    {
        string Quote(string text) => "\"" + text.Replace("\"", "\\\"") + "\"";

        writer.WriteLine("digraph G {");

        var blockIds = new Dictionary<BasicBlock, string>();

        for (int i = 0; i < Blocks.Count; i++)
        {
            var id = $"N{i}";
            blockIds.Add(Blocks[i], id);
        }

        foreach (var block in Blocks)
        {
            var id    = blockIds[block];
            var label = Quote(block.ToString().Replace(Environment.NewLine, "\\l"));
            writer.WriteLine($"    {id} [label = {label} shape = box]");
        }

        foreach (var branch in Branches)
        {
            var fromId = blockIds[branch.From];
            var toId   = blockIds[branch.To];
            var label  = Quote(branch.ToString());
            writer.WriteLine($"    {fromId} -> {toId} [label = {label}]");
        }

        writer.WriteLine("}");
    }

    public static ControlFlowGraph Create(BoundBlockStatement body)
    {
        var basicBlockBuilder = new BasicBlockBuilder();
        var blocks            = basicBlockBuilder.Build(body);

        var graphBuilder = new GraphBuilder();
        return graphBuilder.Build(blocks);
    }

    public static bool AllPathsReturn(BoundBlockStatement body)
    {
        var graph = Create(body);

        foreach (var branch in graph.End.Incoming)
        {
            var lastStatement = branch.From.Statements.LastOrDefault();
            if (lastStatement == null || lastStatement.Kind == BoundNodeKind.ReturnStatement)
                return false;
        }

        return true;
    }

}
