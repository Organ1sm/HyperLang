using System.Collections.Immutable;
using Hyper.Compiler.Binding;
using Hyper.Compiler.Symbol;
using Hyper.Compiler.Syntax;

namespace Compiler.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
    private Lowerer() { }

    private LabelSymbol GenerateLabel() => new LabelSymbol($"Label{++_labelCount}");

    public static BoundBlockStatement Lower(BoundStatement statement)
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
            var gotoFalse         = new BoundConditionalGotoStatement(endLabel, node.Condition, true);
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

            var gotoFalse          = new BoundConditionalGotoStatement(elseLabel, node.Condition, true);
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

    protected override BoundStatement? RewriteWhileStatement(BoundWhileStatement node)
    {
        // while <condition>
        //      <bode>
        //
        // ----->
        //
        // goto check
        // continue:
        // <body>
        // check:
        // gotoTrue <condition> continue
        // end:
        //

        var continueLabel = GenerateLabel();
        var checkLabel    = GenerateLabel();
        var endLabel      = GenerateLabel();

        var gotoCheck              = new BoundGotoStatement(checkLabel);
        var continueLabelStatement = new BoundLabelStatement(continueLabel);
        var checkLabelStatement    = new BoundLabelStatement(checkLabel);

        var gotoTrue          = new BoundConditionalGotoStatement(continueLabel, node.Condition, false);
        var endLabelStatement = new BoundLabelStatement(endLabel);

        var result = new BoundBlockStatement(ImmutableArray.Create(gotoCheck,
                                                                   continueLabelStatement,
                                                                   node.Body,
                                                                   checkLabelStatement,
                                                                   gotoTrue,
                                                                   endLabelStatement));

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
        //      while (<var> <= <upper>)
        //      {
        //          <body>
        //          <var> = <var> + 1
        //      }   
        // }

        var varDecl = new BoundVariableDeclaration(node.Variable, node.LowerBound);
        var varExpr = new BoundVariableExpression(node.Variable);

        var condition = new BoundBinaryExpression(varExpr,
                                                  BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken,
                                                                           typeof(int),
                                                                           typeof(int)),
                                                  node.UpperBound);

        var increment = new BoundExpressionStatement(new BoundAssignmentExpression(node.Variable,
                                                      new BoundBinaryExpression(varExpr,
                                                                                BoundBinaryOperator
                                                                                    .Bind(SyntaxKind.PlusToken,
                                                                                     typeof(int),
                                                                                     typeof(int)),
                                                                                new BoundLiteralExpression(1))));

        var whileBody       = new BoundBlockStatement(ImmutableArray.Create(node.Body, increment));
        var whileStatements = new BoundWhileStatement(condition, whileBody);
        var result          = new BoundBlockStatement(ImmutableArray.Create<BoundStatement?>(varDecl, whileStatements));

        return RewriteStatement(result);
    }

    private int _labelCount;
}
