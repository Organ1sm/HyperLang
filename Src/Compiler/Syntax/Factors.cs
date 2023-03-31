namespace Hyper.Compiler.Syntax;

public static class Factors
{
    public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.StarToken  => 5,
            SyntaxKind.SlashToken => 5,

            SyntaxKind.PlusToken  => 4,
            SyntaxKind.MinusToken => 4,

            SyntaxKind.EqualsEqualsToken    => 3,
            SyntaxKind.BangEqualsToken      => 3,
            SyntaxKind.LessToken            => 3,
            SyntaxKind.LessOrEqualsToken    => 3,
            SyntaxKind.GreaterToken         => 3,
            SyntaxKind.GreaterOrEqualsToken => 3,

            SyntaxKind.AmpersandToken or SyntaxKind.AmpersandAmpersandToken => 2,

            SyntaxKind.HatToken or SyntaxKind.PipeToken or SyntaxKind.PipePipeToken => 1,

            _ => 0
        };
    }

    public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.PlusToken  => 6,
            SyntaxKind.MinusToken => 6,
            SyntaxKind.BangToken  => 6,
            SyntaxKind.TildeToken => 6,
            _                     => 0
        };
    }

    public static SyntaxKind GetKeywordKind(string text)
    {
        return text switch
        {
            "true"  => SyntaxKind.TrueKeyword,
            "false" => SyntaxKind.FalseKeyword,
            "let"   => SyntaxKind.LetKeyword,
            "var"   => SyntaxKind.VarKeyword,
            "if"    => SyntaxKind.IfKeyword,
            "else"  => SyntaxKind.ElseKeyword,
            "do"    => SyntaxKind.DoKeyword,
            "while" => SyntaxKind.WhileKeyword,
            "for"   => SyntaxKind.ForKeyword,
            "to"    => SyntaxKind.ToKeyword,
            "func"  => SyntaxKind.FuncKeyword,
            _       => SyntaxKind.IdentifierToken
        };
    }

    public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
    {
        var kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));
        foreach (var kind in kinds)
        {
            if (GetUnaryOperatorPrecedence(kind) > 0)
                yield return kind;
        }
    }

    public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
    {
        var kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));
        foreach (var kind in kinds)
        {
            if (GetBinaryOperatorPrecedence(kind) > 0)
                yield return kind;
        }
    }

    public static string? GetText(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.PlusToken               => "+",
            SyntaxKind.MinusToken              => "-",
            SyntaxKind.StarToken               => "*",
            SyntaxKind.SlashToken              => "/",
            SyntaxKind.BangToken               => "!",
            SyntaxKind.EqualsToken             => "=",
            SyntaxKind.TildeToken              => "~",
            SyntaxKind.AmpersandToken          => "&",
            SyntaxKind.AmpersandAmpersandToken => "&&",
            SyntaxKind.PipeToken               => "|",
            SyntaxKind.PipePipeToken           => "||",
            SyntaxKind.HatToken                => "^",
            SyntaxKind.EqualsEqualsToken       => "==",
            SyntaxKind.BangEqualsToken         => "!=",
            SyntaxKind.LessToken               => "<",
            SyntaxKind.LessOrEqualsToken       => "<=",
            SyntaxKind.GreaterToken            => ">",
            SyntaxKind.GreaterOrEqualsToken    => ">=",
            SyntaxKind.ArrowToken              => "->",
            SyntaxKind.OpenParenthesisToken    => "(",
            SyntaxKind.CloseParenthesisToken   => ")",
            SyntaxKind.OpenBraceToken          => "{",
            SyntaxKind.CloseBraceToken         => "}",
            SyntaxKind.ColonToken              => ":",
            SyntaxKind.CommaToken              => ",",
            SyntaxKind.FalseKeyword            => "false",
            SyntaxKind.TrueKeyword             => "true",
            SyntaxKind.LetKeyword              => "let",
            SyntaxKind.VarKeyword              => "var",
            SyntaxKind.IfKeyword               => "if",
            SyntaxKind.ElseKeyword             => "else",
            SyntaxKind.DoKeyword               => "do",
            SyntaxKind.WhileKeyword            => "while",
            SyntaxKind.ForKeyword              => "for",
            SyntaxKind.ToKeyword               => "to",
            SyntaxKind.FuncKeyword             => "func",
            _                                  => null
        };
    }
}
