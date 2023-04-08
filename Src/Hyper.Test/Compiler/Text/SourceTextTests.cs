using Hyper.Core.Text;
using Xunit;

namespace Hyper.Test.Compiler.Text;

public class SourceTextTests
{
    [Theory]
    [InlineData(".", 1)]
    [InlineData(".\r\n", 2)]
    [InlineData(".\r\n\r\n", 3)]
    public void SourceText_IncludesLastLine(string text, int expectedLineCount)
    {
        var sourceText = SourceText.MakeSTFrom(text);
        Assert.Equal(expectedLineCount, sourceText.Lines.Length);
    }
}
