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
    OpenBraceToken,          // '{'
    CloseBraceToken,          // '}'

    IdentifierToken,

    // Keywords
    TrueKeyword,
    FalseKeyword,

    // Nodes
    CompilationUnit,

    // Statements
    BlockStatement,
    ExpressionStatement,
    
    // Expression
    LiteralExpression,
    NameExpression,
    BinaryExpression,
    UnaryExpression,
    ParenthesizedExpression,
    AssignmentExpression,
}
