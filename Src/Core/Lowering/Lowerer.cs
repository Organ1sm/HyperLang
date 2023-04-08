﻿using System.Collections.Immutable;
using Hyper.Core.Symbols;
using Hyper.Core.Syntax;
using Hyper.Core.Binding;
using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Operator;
using Hyper.Core.Binding.Stmt;

namespace Hyper.Core.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
    private Lowerer() { }

    private BoundLabel GenerateLabel() => new($"Label{++_labelCount}");

    public static BoundBlockStatement Lower(BoundStatement? statement)
    {
        var lowerer = new Lowerer();
        var result  = lowerer.RewriteStatement(statement);

        return Flatten(result);
    }

    private static BoundBlockStatement Flatten(BoundStatement? statement)
    {
        var builder = ImmutableArray.CreateBuilder<BoundStatement?>();
        var stack   = new Stack<BoundStatement?>();
        stack.Push(statement);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current is BoundBlockStatement block)
            {
                foreach (var s in block.Statements.Reverse())
                {
                    stack.Push(s);
                }
            }
            else
            {
                builder.Add(current);
            }
        }

        return new BoundBlockStatement(builder.ToImmutable());
    }

    protected override BoundStatement? RewriteIfStatement(BoundIfStatement node)
    {
        if (node.ElseStatement == null)
        {
            // if <condition>
            //      <then>
            //
            // ---->
            //
            // gotoFalse <condition> end
            // <then>  
            // end:

            var endLabel          = GenerateLabel();
            var gotoFalse         = new BoundConditionalGotoStatement(endLabel, node.Condition, false);
            var endLabelStatement = new BoundLabelStatement(endLabel);
            var result =
                new BoundBlockStatement(ImmutableArray.Create(gotoFalse,
                                                              node.ThenStatement,
                                                              endLabelStatement));

            return RewriteStatement(result);
        }
        else
        {
            // if <condition>
            //      <then>
            // else
            //      <else>
            //
            // ---->
            //
            // gotoFalse <condition> else
            // <then>
            // goto end
            // else:
            // <else>
            // end:

            var elseLabel = GenerateLabel();
            var endLabel  = GenerateLabel();

            var gotoFalse          = new BoundConditionalGotoStatement(elseLabel, node.Condition, false);
            var gotoEndStatement   = new BoundGotoStatement(endLabel);
            var elseLabelStatement = new BoundLabelStatement(elseLabel);
            var endLabelStatement  = new BoundLabelStatement(endLabel);

            var result =
                new BoundBlockStatement(ImmutableArray.Create(gotoFalse,
                                                              node.ThenStatement,
                                                              gotoEndStatement,
                                                              elseLabelStatement,
                                                              node.ElseStatement,
                                                              endLabelStatement));

            return RewriteStatement(result);
        }
    }


    protected override BoundStatement? RewriteDoWhileStatement(BoundDoWhileStatement node)
    {
        // do
        //    <body>
        // while <condition>
        //
        // ----->
        //
        // body:
        // <body>
        // continue:
        // gotoTrue <condition> body
        // break:
        //
        var bodyLabel = GenerateLabel();

        var bodyLabelStatement     = new BoundLabelStatement(bodyLabel);
        var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);
        var gotoTrueStatement      = new BoundConditionalGotoStatement(bodyLabel, node.Condition);
        var breakLabelStatement    = new BoundLabelStatement(node.BreakLabel);

        var result = new BoundBlockStatement(ImmutableArray.Create(bodyLabelStatement,
                                                                   node.Body,
                                                                   continueLabelStatement,
                                                                   gotoTrueStatement,
                                                                   breakLabelStatement));

        return RewriteStatement(result);
    }

    protected override BoundStatement? RewriteWhileStatement(BoundWhileStatement node)
    {
        // while <condition>
        //      <body>
        //
        // ----->
        //
        // goto continue
        // body:
        // <body>
        // continue:
        // gotoTrue <condition> body
        // break:

        var bodyLabel = GenerateLabel();

        var gotoContinue           = new BoundGotoStatement(node.ContinueLabel);
        var bodyLabelStatement     = new BoundLabelStatement(bodyLabel);
        var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);

        var gotoTrue            = new BoundConditionalGotoStatement(bodyLabel, node.Condition);
        var breakLabelStatement = new BoundLabelStatement(node.BreakLabel);

        var result = new BoundBlockStatement(ImmutableArray.Create(gotoContinue,
                                                                   bodyLabelStatement,
                                                                   node.Body,
                                                                   continueLabelStatement,
                                                                   gotoTrue,
                                                                   breakLabelStatement));

        return RewriteStatement(result);
    }

    protected override BoundStatement? RewriteForStatement(BoundForStatement node)
    {
        // for <var> = <lower> to <upper>
        //      <body>
        //
        // ---->
        //
        // {
        //      var <var> = <lower>
        //      let upperBound = <upper>
        //      while (<var> <= <upperBound>)
        //      {
        //          <body>
        //          continue:
        //          <var> = <var> + 1
        //      }   
        // }

        var varDecl = new BoundVariableDeclaration(node.Variable, node.LowerBound);
        var varExpr = new BoundVariableExpression(node.Variable);

        var upperBoundSymbol = new LocalVariableSymbol("upperBound", TypeSymbol.Int, true);
        var upperBoundDecl   = new BoundVariableDeclaration(upperBoundSymbol, node.UpperBound);

        var condition = new BoundBinaryExpression(varExpr,
                                                  BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken,
                                                                           TypeSymbol.Int,
                                                                           TypeSymbol.Int),
                                                  new BoundVariableExpression(upperBoundSymbol));

        var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);

        var increment = new BoundExpressionStatement(new BoundAssignmentExpression(node.Variable,
                                                      new BoundBinaryExpression(varExpr,
                                                                                BoundBinaryOperator
                                                                                   .Bind(SyntaxKind.PlusToken,
                                                                                     TypeSymbol.Int,
                                                                                     TypeSymbol.Int),
                                                                                new BoundLiteralExpression(1))));

        var whileBody = new BoundBlockStatement(ImmutableArray.Create(node.Body, continueLabelStatement, increment));
        var whileStatements = new BoundWhileStatement(condition, whileBody, node.BreakLabel, GenerateLabel());
        var result =
            new BoundBlockStatement(ImmutableArray.Create<BoundStatement?>(varDecl, upperBoundDecl, whileStatements));

        return RewriteStatement(result);
    }

    private int _labelCount;
}
