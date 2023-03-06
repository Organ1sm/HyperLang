namespace HyperLang.SimpleDemo;

enum SyntaxKind
{
    NumberToken,
    WhitespaceToken,
    PlusToken,             // +
    MinusToken,            // -
    StarToken,             // *
    SlashToken,            // /
    OpenParenthesisToken,  // '('
    CloseParenthesisToken, // ')'
    BadToken,
    EndOfFileToken,
    NumberExpression,
    BinaryExpression,
    ParenthesizedExpression
}
