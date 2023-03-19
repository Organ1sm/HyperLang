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
    LessToken,               // '<'
    LessOrEqualsToken,       // '<='
    GreaterToken,            // '>'
    GreaterOrEqualsToken,    // '>='
    OpenParenthesisToken,    // '('
    CloseParenthesisToken,   // ')'
    OpenBraceToken,          // '{'
    CloseBraceToken,         // '}'
    ColonToken,

    IdentifierToken,

    // Keywords
    TrueKeyword,
    FalseKeyword,
    LetKeyword,
    VarKeyword,
    IfKeyword,
    ElseKeyword,
    WhileKeyword,
    

    // Nodes
    CompilationUnit,
    ElseClause,
    

    // Statements
    BlockStatement,
    VariableDeclaration,
    ExpressionStatement,
    IfStatement,
    WhileStatement,
    

    // Expression
    LiteralExpression,
    NameExpression,
    BinaryExpression,
    UnaryExpression,
    ParenthesizedExpression,
    AssignmentExpression,
}
