using Hyper.Compiler.Syntax;
using Xunit;

namespace Hyper.Test.Compiler.Syntax;

public class FactorsTests
{
    [Theory]
    [MemberData(nameof(GetSyntaxKindData))]
    public void FactorsGetTextTest(SyntaxKind kind)
    {
        var text = Factors.GetText(kind);
        if (text == null)
            return;

        var tokens = AST.ParseTokens(text);
        var token  = Assert.Single(tokens);

        Assert.Equal(kind, token.Kind);
        Assert.Equal(text, token.Text);
    }

    public static IEnumerable<object[]> GetSyntaxKindData()
    {
        var kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));

        foreach (var kind in kinds)
            yield return new object[] {kind};
    }
}
