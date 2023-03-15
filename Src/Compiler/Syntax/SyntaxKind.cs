namespace Hyper.Compiler.Syntax;

public enum SyntaxKind
{
    BadToken,
    EndOfFileToken,

    WhitespaceToken,
    NumberToken,

    PlusToken,               // +
    MinusToken,              // -
    StarToken,               // *
    SlashToken,              // /
    BangToken,               // '!'
    EqualsToken,             // '='
    AmpersandAmpersandToken, // '&&'
    PipePipeToken,           // '||'
    EqualsEqualsToken,       // '=='
    BangEqualsToken,         // '!='
    OpenParenthesisToken,    // '('
    CloseParenthesisToken,   // ')'

    IdentifierToken,

    // Keywords
    TrueKeyword,
    FalseKeyword,
    
    // Nodes
    CompilationUnit,

    // Expression
    LiteralExpression,
    NameExpression,
    BinaryExpression,
    UnaryExpression,
    ParenthesizedExpression,
    AssignmentExpression,
}
