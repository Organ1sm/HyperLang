using Hyper.Core.Symbols;
using Hyper.Core.Syntax;
using Hyper.Core.VM;
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
    [InlineData("\"test\"", "test")]
    [InlineData("\"te\"\"st\"", "te\"st")]
    [InlineData("\"test\" == \"test\"", true)]
    [InlineData("\"test\" != \"test\"", false)]
    [InlineData("\"test\" == \"abc\"", false)]
    [InlineData("\"test\" != \"abc\"", true)]
    [InlineData("\"test\" + \"abc\"", "testabc")]
    [InlineData("{ var a = 10 (a * a) }", 100)]
    [InlineData("{ var a = 0 (a = 10) * a }", 100)]
    [InlineData("{ var a = 0 if a == 0: a = 10 a }", 10)]
    [InlineData("{ var a = 0 if a == 4: a = 10 a }", 0)]
    [InlineData("{ var a = 0 if a == 0: a = 10 else: a = 5 a }", 10)]
    [InlineData("{ var a = 0 if a == 4: a = 10 else: a = 5 a }", 5)]
    [InlineData("{ var i = 10 var result = 0 while i > 0: { result = result + i i = i - 1} result }", 55)]
    [InlineData("{ var result = 0 for i = 1 to 10 { result = result + i } result }", 55)]
    [InlineData("{ var a = 0 do: a = a + 1 while a < 10 a}", 10)]
    [InlineData("{ var i = 0 while i < 5: { i = i + 1 if i == 5: continue } i }", 5)]
    [InlineData("{ var i = 0 do: { i = i + 1 if i == 5: continue } while i < 5 i } ", 5)]
    public void EvaluatorComputesCorrectValues(string text, object expectedValue) => AssertValue(text, expectedValue);


    [Fact]
    public void EvaluatorInvokeFunctionArgumentsMissing()
    {
        var text = @"
                print([)]
            ";

        var diagnostics = @"
                Function 'print' requires 1 arguments but was given 0.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorInvokeFunctionArgumentsExceeding()
    {
        var text = @"
                print(""Hello""[, "" "", "" world!""])
            ";

        var diagnostics = @"
                Function 'print' requires 1 arguments but was given 3.
            ";

        AssertDiagnostics(text, diagnostics);
    }

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
    public void EvaluatorInvokeFunctionArgumentsNoInfiniteLoop()
    {
        var text = @"
                print(""Hi""[[=]][)]
            ";

        var diagnostics = @"
                Unexpected token <EqualsToken>, expected <CloseParenthesisToken>.
                Unexpected token <EqualsToken>, expected <IdentifierToken>.
                Unexpected token <CloseParenthesisToken>, expected <IdentifierToken>.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorFunctionParametersNoInfiniteLoop()
    {
        var text = @"
                func hi(name: string[[[=]]][)]
                {
                    print(""Hi "" + name + ""!"" )
                }[]

            ";

        var diagnostics = @"
                Unexpected token <EqualsToken>, expected <CloseParenthesisToken>.
                Unexpected token <EqualsToken>, expected <OpenBraceToken>.
                Unexpected token <EqualsToken>, expected <IdentifierToken>.
                Unexpected token <CloseParenthesisToken>, expected <IdentifierToken>.
                Unexpected token <EndOfFileToken>, expected <CloseBraceToken>.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorNameExpressionReportsNoErrorForInsertedToken()
    {
        var text = @"1 + []";

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

    [Fact]
    public void EvaluatorVoidFunctionShouldNotReturnValue()
    {
        var text = @"
                func test()
                {
                    return [1]
                }
            ";

        var diagnostics = @"
                Since the function 'test' does not return a value the 'return' keyword cannot be followed by an expression.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorFunctionWithReturnValueShouldNotReturnVoid()
    {
        var text = @"
                func test()-> int
                {
                    [return]
                }
            ";

        var diagnostics = @"
                An expression of type 'int' expected.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorNotAllCodePathsReturnValue()
    {
        var text = @"
                func [test](n: int)-> bool
                {
                    if (n > 10):
                       return true
                }
            ";

        var diagnostics = @"
                Not all code paths return a value.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorExpressionMustHaveValue()
    {
        var text = @"
                func test(n: int)
                {
                    return
                }

                let value = [test(100)]
            ";

        var diagnostics = @"
                Expression must have a value.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Theory]
    [InlineData("[break]", "break")]
    [InlineData("[continue]", "continue")]
    public void EvaluatorInvalidBreakOrContinue(string text, string keyword)
    {
        var diagnostics = $@"
                The keyword '{keyword}' can only be used inside of loops.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorInvalidReturn()
    {
        var text = @"
                [return]
            ";

        var diagnostics = @"
                The 'return' keyword can only be used inside of functions.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorParameterAlreadyDeclared()
    {
        var text = @"
                func sum(a: int, b: int, [a: int])-> int
                {
                    return a + b + c
                }
            ";

        var diagnostics = @"
                A parameter with the name 'a' already exists.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorFunctionMustHaveName()
    {
        var text = @"
                func [(]a: int, b: int)-> int
                {
                    return a + b
                }
            ";

        var diagnostics = @"
                Unexpected token <OpenParenthesisToken>, expected <IdentifierToken>.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorWrongArgumentType()
    {
        var text = @"
                func test(n: int)-> bool
                {
                    return n > 10
                }
                let testValue = ""string""
                test([testValue])
            ";

        var diagnostics = @"
                Parameter 'n' requires a value of type 'int' but was given a value of type 'string'.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void EvaluatorBadType()
    {
        var text = @"
                func test(n: [invalidtype])
                {
                }
            ";

        var diagnostics = @"
                Type 'invalidtype' doesn't exist.
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
