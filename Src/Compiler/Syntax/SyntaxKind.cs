namespace Hyper.Compiler.Syntax;

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
    
    IdentifierToken,
    
    // Keywords
    TrueKeyword,
    FalseKeyword,

    // Expression
    LiteralExpression,
    BinaryExpression,
    UnaryExpression,
    ParenthesizedExpression,
}
