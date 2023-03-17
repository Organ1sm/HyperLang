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
    LetKeyword,
    VarKeyword,

    // Nodes
    CompilationUnit,

    // Statements
    BlockStatement,
    VariableDeclaration,
    ExpressionStatement,
    
    // Expression
    LiteralExpression,
    NameExpression,
    BinaryExpression,
    UnaryExpression,
    ParenthesizedExpression,
    AssignmentExpression,
}
