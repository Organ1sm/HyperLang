﻿using Hyper.Compiler.Symbols;
using Hyper.Compiler.Syntax;
using Hyper.Compiler.VM;
using Hyper.Test.Compiler.Text;
using Xunit;

namespace Hyper.Test.Compiler.Syntax;

public class EvaluationTests
{
    [Theory]
    [InlineData("1", 1)]
    [InlineData("+1", 1)]
    [InlineData("-1", -1)]
    [InlineData("~1", -2)]
    [InlineData("14 + 12", 26)]
    [InlineData("12 - 3", 9)]
    [InlineData("4 * 2", 8)]
    [InlineData("9 / 3", 3)]
    [InlineData("(10)", 10)]
    [InlineData("12 == 3", false)]
    [InlineData("3 == 3", true)]
    [InlineData("12 != 3", true)]
    [InlineData("3 != 3", false)]
    [InlineData("3 < 4", true)]
    [InlineData("5 < 4", false)]
    [InlineData("4 <= 4", true)]
    [InlineData("4 <= 5", true)]
    [InlineData("5 <= 4", false)]
    [InlineData("4 > 3", true)]
    [InlineData("4 > 5", false)]
    [InlineData("4 >= 4", true)]
    [InlineData("5 >= 4", true)]
    [InlineData("4 >= 5", false)]
    [InlineData("1 | 2", 3)]
    [InlineData("1 | 0", 1)]
    [InlineData("1 & 3", 1)]
    [InlineData("1 & 0", 0)]
    [InlineData("1 ^ 0", 1)]
    [InlineData("0 ^ 1", 1)]
    [InlineData("1 ^ 3", 2)]
    [InlineData("false == false", true)]
    [InlineData("true == false", false)]
    [InlineData("false != false", false)]
    [InlineData("true != false", true)]
    [InlineData("true && true", true)]
    [InlineData("false || false", false)]
    [InlineData("false | false", false)]
    [InlineData("false | true", true)]
    [InlineData("true | false", true)]
    [InlineData("true | true", true)]
    [InlineData("false & false", false)]
    [InlineData("false & true", false)]
    [InlineData("true & false", false)]
    [InlineData("true & true", true)]
    [InlineData("false ^ false", false)]
    [InlineData("true ^ false", true)]
    [InlineData("false ^ true", true)]
    [InlineData("true ^ true", false)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("!true", false)]
    [InlineData("!false", true)]
    [InlineData("var a = 10", 10)]
    [InlineData("{ var a = 10 (a * a) }", 100)]
    [InlineData("{ var a = 0 (a = 10) * a }", 100)]
    [InlineData("{ var a = 0 if a == 0: a = 10 a }", 10)]
    [InlineData("{ var a = 0 if a == 4: a = 10 a }", 0)]
    [InlineData("{ var a = 0 if a == 0: a = 10 else: a = 5 a }", 10)]
    [InlineData("{ var a = 0 if a == 4: a = 10 else: a = 5 a }", 5)]
    [InlineData("{ var i = 10 var result = 0 while i > 0: { result = result + i i = i - 1} result }", 55)]
    [InlineData("{ var result = 0 for i = 1 to 10 { result = result + i } result }", 55)]
    [InlineData("{ var a = 0 do: a = a + 1 while a < 10 a}", 10)]
    public void EvaluatorComputesCorrectValues(string text, object expectedValue) => AssertValue(text, expectedValue);

    [Fact]
    public void EvaluatorVariableDeclarationReportsRedeclaration()
    {
        var text = @"
            {
                var x = 10
                var y = 100
                { 
                    var x = 10
                }
                var [x] = 5
            }
        ";

        var diagnostics = @"
            'x' is already declared.
        ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorBlockStatementNoInfiniteLoop()
    {
        var text = @"
            {
            [)][]
        ";

        var diagnostics = @"
            Unexpected token <CloseParenthesisToken>, expected <IdentifierToken>.
            Unexpected token <EndOfFileToken>, expected <CloseBraceToken>.
        ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorNameExpressionReportsNoErrorForInsertedToken()
    {
        var text = @"[]";

        var diagnostics = @"
                Unexpected token <EndOfFileToken>, expected <IdentifierToken>.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorIfStatementReportsCannotConvert()
    {
        var text = @"
            {
                var x = 0
                if [10]:
                    x = 10
            }
        ";

        var diagnostics = @"
                Cannot convert type 'int' to 'bool'.
        ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorWhileStatementReportsCannotConvert()
    {
        var text = @"
            {
                var x = 0
                while [10]:
                    x = 10
        }";

        var diagnostics = @"
                Cannot convert type 'int' to 'bool'.
        ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorDoWhileStatementReportsCannotConvert()
    {
        var text = @"
                {
                    var x = 0
                    do:
                        x = 10
                    while [10]
                }
            ";

        var diagnostics = @"
                Cannot convert type 'int' to 'bool'.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorForStatementReportsCannotConvert()
    {
        var text = @"
            {
                var result = 0
                for i = 1 to [true]
                    result = result + i
            }";

        var diagnostics = @"
                Cannot convert type 'bool' to 'int'.
        ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorNameExpressionReportsUndefined()
    {
        var text = @"[x] * 10";
        var diagnostics = @"
                Variable 'x' doesn't exist.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorBinaryExpressionReportsUndefined()
    {
        var text = @"10 [*] false";

        var diagnostics = @"
                Binary operator '*' is not defined for types 'int' and 'bool'.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorAssignmentExpressionReportsCannotAssign()
    {
        var text = @"
                {
                    let x = 10
                    x [=] 0
                }
            ";

        var diagnostics = @"
                Variable 'x' is read-only and cannot be assigned to.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorAssignmentExpressionReportsCannotConvert()
    {
        var text = @"
                {
                    var x = 10
                    x = [true]
                }
            ";

        var diagnostics = @"
                Cannot convert type 'bool' to 'int'.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorVariablesCanShadowFunctions()
    {
        var text = @"
                {
                    let print = 42
                    [print](""test"")
                }
            ";

        var diagnostics = @"
                Function 'print' doesn't exist.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    private static void AssertValue(string text, object expectedValue)
    {
        var ast         = AST.Parse(text);
        var compilation = new Compilation(ast);
        var variables   = new Dictionary<VariableSymbol, object>();
        var result      = compilation.Evaluate(variables);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(expectedValue, result.Value);
    }

    private void AssertDiagnostics(string text, string diagnosticText)
    {
        var annotatedText = AnnotatedText.Parse(text);
        var syntaxTree    = AST.Parse(annotatedText.Text);
        var compilation   = new Compilation(syntaxTree);
        var result        = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        var expectedDiagnostics = AnnotatedText.ReduceIndentWithLines(diagnosticText);

        if (annotatedText.Spans.Length != expectedDiagnostics.Length)
            throw new Exception("Error: Must mark as many spans as there are expected diagnostics");

        Assert.Equal(expectedDiagnostics.Length, result.Diagnostics.Length);

        for (var i = 0; i < expectedDiagnostics.Length; i++)
        {
            var expectedMessage = expectedDiagnostics[i];
            var actualMessage   = result.Diagnostics[i].Message;
            Assert.Equal(expectedMessage, actualMessage);

            var expectedSpan = annotatedText.Spans[i];
            var actualSpan   = result.Diagnostics[i].Span;
            Assert.Equal(expectedSpan, actualSpan);
        }
    }
}
