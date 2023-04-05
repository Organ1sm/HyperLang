namespace Hyper.Core.Syntax;

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
    ArrowToken,              // '->'
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
    DoKeyword,
    WhileKeyword,
    ForKeyword,
    ToKeyword,
    FuncKeyword,


    // Nodes
    CompilationUnit,
    FunctionDeclaration,
    GlobalStatement,
    Parameter,
    ElseClause,
    TypeClause,

    // Statements
    BlockStatement,
    VariableDeclaration,
    ExpressionStatement,
    IfStatement,
    WhileStatement,
    DoWhileStatement,
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
