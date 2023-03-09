namespace Hyper.Compiler.Syntax;

internal static class Factors
{
    public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.StarToken               => 5,
            SyntaxKind.SlashToken              => 5,
            
            SyntaxKind.PlusToken               => 4,
            SyntaxKind.MinusToken              => 4,
            
            SyntaxKind.EqualsEqualsToken       => 3,
            SyntaxKind.BangEqualsToken         => 3,
            
            SyntaxKind.AmpersandAmpersandToken => 2,
            SyntaxKind.PipePipeToken           => 1,
            _                                  => 0
        };
    }

    public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.PlusToken  => 6,
            SyntaxKind.MinusToken => 6,
            SyntaxKind.BangToken  => 6,
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
