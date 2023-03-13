﻿using System.Collections.Specialized;
using Hyper.Compiler.Syntax;
using Xunit;

namespace Hyper.Test.Compiler.Syntax;

public class ParserTests
{
    [Theory]
    [MemberData(nameof(GetBinaryOperatorPairsData))]
    public void ParserBinaryExpressionPrecedences(SyntaxKind op1, SyntaxKind op2)
    {
        var op1Precedence = Factors.GetBinaryOperatorPrecedence(op1);
        var op2Precedence = Factors.GetBinaryOperatorPrecedence(op2);
        var op1Text       = Factors.GetText(op1);
        var op2Text       = Factors.GetText(op2);

        var text       = $"a {op1Text} b {op2Text} c";
        var expression = AST.Parse(text).Root;

        if (op1Precedence >= op2Precedence)
        {
            //     op2
            //    /   \
            //   op1   c
            //  /   \
            // a     b
            using (var e = new AssertingEnumerator(expression))
            {
                e.AssertNode(SyntaxKind.BinaryExpression);
                e.AssertNode(SyntaxKind.BinaryExpression);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "a");
                e.AssertToken(op1, op1Text);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "b");
                e.AssertToken(op2, op2Text);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "c");
            }
        }
        else
        {
            //   op1
            //  /   \
            // a    op2
            //     /   \
            //    b     c

            using (var e = new AssertingEnumerator(expression))
            {
                e.AssertNode(SyntaxKind.BinaryExpression);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "a");
                e.AssertToken(op1, op1Text);
                e.AssertNode(SyntaxKind.BinaryExpression);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "b");
                e.AssertToken(op2, op2Text);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "c");
            }
        }
    }

    [Theory]
    [MemberData(nameof(GetUnaryOperatorPairsData))]
    public void ParserUnaryExpressionPrecedences(SyntaxKind unaryKind, SyntaxKind binaryKind)
    {
        var unaryOpPrecedence  = Factors.GetUnaryOperatorPrecedence(unaryKind);
        var binaryOpPrecedence = Factors.GetBinaryOperatorPrecedence(binaryKind);
        var unaryOpText        = Factors.GetText(unaryKind);
        var binaryOpText       = Factors.GetText(binaryKind);

        var text       = $"{unaryOpText} a {binaryOpText} b";
        var expression = AST.Parse(text).Root;

        if (unaryOpPrecedence >= binaryOpPrecedence)
        {
            //   binary
            //   /    \
            // unary   b
            //   |
            //   a

            using (var e = new AssertingEnumerator(expression))
            {
                e.AssertNode(SyntaxKind.BinaryExpression);
                e.AssertNode(SyntaxKind.UnaryExpression);
                e.AssertToken(unaryKind, unaryOpText);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "a");
                e.AssertToken(binaryKind, binaryOpText);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "b");
            }
        }
        else
        {
            //  unary
            //    |
            //  binary
            //  /   \
            // a     b

            using (var e = new AssertingEnumerator(expression))
            {
                e.AssertNode(SyntaxKind.UnaryExpression);
                e.AssertToken(unaryKind, unaryOpText);
                e.AssertNode(SyntaxKind.BinaryExpression);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "a");
                e.AssertToken(binaryKind, binaryOpText);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "b");
            }
        }
    }

    public static IEnumerable<object[]> GetBinaryOperatorPairsData()
    {
        foreach (var op1 in Factors.GetBinaryOperatorKinds())
        {
            foreach (var op2 in Factors.GetBinaryOperatorKinds())
            {
                yield return new object[] {op1, op2};
            }
        }
    }

    public static IEnumerable<object[]> GetUnaryOperatorPairsData()
    {
        foreach (var unaryOp in Factors.GetUnaryOperatorKinds())
        {
            foreach (var binaryOp in Factors.GetBinaryOperatorKinds())
            {
                yield return new object[] {unaryOp, binaryOp};
            }
        }
    }
}
