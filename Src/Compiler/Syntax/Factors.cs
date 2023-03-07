namespace Hyper.Compiler.Syntax;

internal static class Factors
{
    public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.StarToken  => 2,
            SyntaxKind.SlashToken => 2,
            SyntaxKind.PlusToken  => 1,
            SyntaxKind.MinusToken => 1,
            _                     => 0
        };
    }

    public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.PlusToken  => 3,
            SyntaxKind.MinusToken => 3,
            _                     => 0
        };
    }

    public static SyntaxKind GetKeywordKind(string text)
    {
        return text switch
        {
            "true"  => SyntaxKind.TrueKeyword,
            "false" => SyntaxKind.FalseKeyword,
            _       => SyntaxKind.IdentifierToken
        };
    }
}
