namespace Hyper.Compiler.Syntax;

public enum SyntaxKind
{
    BadToken,
    EndOfFileToken,

    WhitespaceToken,
    NumberToken,
    StringToken,

    PlusToken,               // +
    MinusToken,              // -
    StarToken,               // *
    SlashToken,              // /
    BangToken,               // '!'
    EqualsToken,             // '='
    TildeToken,              // '~'
    HatToken,                // '^'
    AmpersandToken,          // '&'
    AmpersandAmpersandToken, // '&&'
    PipeToken,               // '|'
    PipePipeToken,           // '||'
    EqualsEqualsToken,       // '=='
    BangEqualsToken,         // '!='
    LessToken,               // '<'
    LessOrEqualsToken,       // '<='
    GreaterToken,            // '>'
    GreaterOrEqualsToken,    // '>='
    OpenParenthesisToken,    // '('
    CloseParenthesisToken,   // ')'
    OpenBraceToken,          // '{'
    CloseBraceToken,         // '}'
    ColonToken,              // ':',
    CommaToken,              // ','

    IdentifierToken,

    // Keywords
    TrueKeyword,
    FalseKeyword,
    LetKeyword,
    VarKeyword,
    IfKeyword,
    ElseKeyword,
    WhileKeyword,
    ForKeyword,
    ToKeyword,


    // Nodes
    CompilationUnit,
    ElseClause,


    // Statements
    BlockStatement,
    VariableDeclaration,
    ExpressionStatement,
    IfStatement,
    WhileStatement,
    ForStatement,

    // Expression
    LiteralExpression,
    NameExpression,
    BinaryExpression,
    UnaryExpression,
    ParenthesizedExpression,
    AssignmentExpression,
    CallExpression,
}
