using System.Collections.Immutable;
using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Operator;
using Hyper.Core.Binding.Stmt;

namespace Hyper.Core.Binding;

internal class BoundTreeRewriter
{
    protected virtual BoundStatement? RewriteStatement(BoundStatement? node)
    {
        return node?.Kind switch
        {
            BoundNodeKind.BlockStatement      => RewriteBlockStatement((BoundBlockStatement) node),
            BoundNodeKind.VariableDeclaration => RewriteVariableDeclaration((BoundVariableDeclaration) node),
            BoundNodeKind.IfStatement         => RewriteIfStatement((BoundIfStatement) node),
            BoundNodeKind.DoWhileStatement    => RewriteDoWhileStatement((BoundDoWhileStatement) node),
            BoundNodeKind.WhileStatement      => RewriteWhileStatement((BoundWhileStatement) node),
            BoundNodeKind.ForStatement        => RewriteForStatement((BoundForStatement) node),
            BoundNodeKind.LabelStatement      => RewriteLabelStatement((BoundLabelStatement) node),
            BoundNodeKind.GotoStatement       => RewriteGotoStatement((BoundGotoStatement) node),
            BoundNodeKind.ConditionalGotoStatement =>
                RewriteConditionalGotoStatement((BoundConditionalGotoStatement) node),
            BoundNodeKind.ExpressionStatement => RewriteExpressionStatement((BoundExpressionStatement) node),
            _                                 => throw new Exception($"Unexpected node: {node?.Kind}")
        };
    }

    protected virtual BoundExpression RewriteExpression(BoundExpression node)
    {
        return node.Kind switch
        {
            BoundNodeKind.LiteralExpression    => RewriteLiteralExpression((BoundLiteralExpression) node),
            BoundNodeKind.VariableExpression   => RewriteVariableExpression((BoundVariableExpression) node),
            BoundNodeKind.AssignmentExpression => RewriteAssignmentExpression((BoundAssignmentExpression) node),
            BoundNodeKind.UnaryExpression      => RewriteUnaryExpression((BoundUnaryExpression) node),
            BoundNodeKind.BinaryExpression     => RewriteBinaryExpression((BoundBinaryExpression) node),
            BoundNodeKind.CallExpression       => RewriteCallExpression((BoundCallExpression) node),
            BoundNodeKind.ConversionExpression => RewriteConversionExpression((BoundConversionExpression) node),
            _                                  => throw new Exception($"Unexpected node: {node.Kind}")
        };
    }

    protected virtual BoundStatement? RewriteBlockStatement(BoundBlockStatement node)
    {
        ImmutableArray<BoundStatement?>.Builder? builder = null;

        for (var i = 0; i < node.Statements.Length; i++)
        {
            var oldStatement = node.Statements[i];
            var newStatement = RewriteStatement(oldStatement);

            if (newStatement != oldStatement)
            {
                if (builder == null)
                {
                    builder = ImmutableArray.CreateBuilder<BoundStatement?>(node.Statements.Length);

                    for (var j = 0; j < i; j++)
                        builder.Add(node.Statements[j]);
                }
            }

            if (builder != null)
                builder.Add(newStatement);
        }

        if (builder == null)
            return node;

        return new BoundBlockStatement(builder.MoveToImmutable());
    }

    protected virtual BoundStatement? RewriteVariableDeclaration(BoundVariableDeclaration node)
    {
        var initializer = RewriteExpression(node.Initializer);

        return initializer == node.Initializer ? node : new BoundVariableDeclaration(node.Variable, initializer);
    }

    protected virtual BoundStatement? RewriteIfStatement(BoundIfStatement node)
    {
        var condition     = RewriteExpression(node.Condition);
        var thenStatement = RewriteStatement(node.ThenStatement);
        var elseStatement = node.ElseStatement == null ? null : RewriteStatement(node.ElseStatement);

        if (condition == node.Condition && thenStatement == node.ThenStatement && elseStatement == node.ElseStatement)
            return node;

        return new BoundIfStatement(condition, thenStatement, elseStatement);
    }

    protected virtual BoundStatement? RewriteDoWhileStatement(BoundDoWhileStatement node)
    {
        var body      = RewriteStatement(node.Body);
        var condition = RewriteExpression(node.Condition);

        if (condition == node.Condition && body == node.Body)
            return node;

        return new BoundDoWhileStatement(body, condition, node.BreakLabel, node.ContinueLabel);
    }

    protected virtual BoundStatement? RewriteWhileStatement(BoundWhileStatement node)
    {
        var condition = RewriteExpression(node.Condition);
        var body      = RewriteStatement(node.Body);

        if (condition == node.Condition && body == node.Body)
            return node;

        return new BoundWhileStatement(condition, body, node.BreakLabel, node.ContinueLabel);
    }

    protected virtual BoundStatement? RewriteForStatement(BoundForStatement node)
    {
        var lowerBound = RewriteExpression(node.LowerBound);
        var upperBound = RewriteExpression(node.UpperBound);
        var body       = RewriteStatement(node.Body);

        if (lowerBound == node.LowerBound && upperBound == node.UpperBound && body == node.Body)
            return node;

        return new BoundForStatement(node.Variable, lowerBound, upperBound, body, node.BreakLabel, node.ContinueLabel);
    }

    protected virtual BoundStatement? RewriteExpressionStatement(BoundExpressionStatement node)
    {
        var expression = RewriteExpression(node.Expression);
        return expression == node.Expression ? node : new BoundExpressionStatement(expression);
    }

    protected virtual BoundStatement? RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
    {
        var condition = RewriteExpression(node.Condition);
        return condition == node.Condition
            ? node
            : new BoundConditionalGotoStatement(node.Label, condition, node.JumpIfTrue);
    }

    protected virtual BoundStatement RewriteLabelStatement(BoundLabelStatement node) => node;
    protected virtual BoundStatement RewriteGotoStatement(BoundGotoStatement node) => node;


    protected virtual BoundExpression RewriteErrorExpression(BoundErrorExpression node) => node;
    protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node) => node;
    protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression node) => node;

    protected virtual BoundExpression RewriteAssignmentExpression(BoundAssignmentExpression node)
    {
        var expression = RewriteExpression(node.Expression);
        return expression == node.Expression ? node : new BoundAssignmentExpression(node.Variable, expression);
    }

    protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
    {
        var operand = RewriteExpression(node.Operand);
        return operand == node.Operand ? node : new BoundUnaryExpression(node.Operator, operand);
    }

    protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
    {
        var left  = RewriteExpression(node.Left);
        var right = RewriteExpression(node.Right);
        if (left == node.Left && right == node.Right)
            return node;

        return new BoundBinaryExpression(left, node.Operator, right);
    }

    protected virtual BoundExpression RewriteCallExpression(BoundCallExpression node)
    {
        ImmutableArray<BoundExpression>.Builder builder = null;

        for (var i = 0; i < node.Arguments.Length; i++)
        {
            var oldArgument = node.Arguments[i];
            var newArgument = RewriteExpression(oldArgument);
            if (newArgument != oldArgument)
            {
                if (builder == null)
                {
                    builder = ImmutableArray.CreateBuilder<BoundExpression>(node.Arguments.Length);

                    for (var j = 0; j < i; j++)
                        builder.Add(node.Arguments[j]);
                }
            }

            if (builder != null)
                builder.Add(newArgument);
        }

        return builder == null ? node : new BoundCallExpression(node.Function, builder.MoveToImmutable());
    }

    protected virtual BoundExpression RewriteConversionExpression(BoundConversionExpression node)
    {
        var expression = RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;

        return new BoundConversionExpression(node.Type, expression);
    }
}
