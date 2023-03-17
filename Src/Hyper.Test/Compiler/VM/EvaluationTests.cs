using Hyper.Compiler.Symbol;
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
    [InlineData("14 + 12", 26)]
    [InlineData("12 - 3", 9)]
    [InlineData("4 * 2", 8)]
    [InlineData("9 / 3", 3)]
    [InlineData("(10)", 10)]
    [InlineData("12 == 3", false)]
    [InlineData("3 == 3", true)]
    [InlineData("12 != 3", true)]
    [InlineData("3 != 3", false)]
    [InlineData("false == false", true)]
    [InlineData("true == false", false)]
    [InlineData("false != false", false)]
    [InlineData("true != false", true)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("!true", false)]
    [InlineData("!false", true)]
    [InlineData("{ var a = 0 (a = 10) * a }", 100)]
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
            Variable 'x' is already declared.
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
