namespace HyperLang.SimpleDemo;

public enum SyntaxKind
{
    
    BadToken,
    EndOfFileToken,
    
    WhitespaceToken,
    NumberToken,
    
    PlusToken,             // +
    MinusToken,            // -
    StarToken,             // *
    SlashToken,            // /
    OpenParenthesisToken,  // '('
    CloseParenthesisToken, // ')'
    
    // Expression
    LiteralExpression,
    BinaryExpression,
    ParenthesizedExpression
}
