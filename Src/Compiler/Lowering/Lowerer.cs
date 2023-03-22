﻿using System.Collections.Immutable;
using Hyper.Compiler.Binding;
using Hyper.Compiler.Syntax;

namespace Compiler.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
    private Lowerer() { }

    public static BoundStatement? Lower(BoundStatement statement)
    {
        var lowerer = new Lowerer();
        return lowerer.RewriteStatement(statement);
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

        var whileBody       = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(node.Body, increment));
        var whileStatements = new BoundWhileStatement(condition, whileBody);
        var result          = new BoundBlockStatement(ImmutableArray.Create<BoundStatement?>(varDecl, whileStatements));

        return RewriteStatement(result);
    }
}