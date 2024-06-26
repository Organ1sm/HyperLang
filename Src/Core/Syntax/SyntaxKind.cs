﻿namespace Hyper.Core.Syntax;

public enum SyntaxKind
{
    EndOfFileToken,

    BadTokenTrivia,
    WhitespaceTrivia,
    SingleLineCommentTrivia,
    MultiLineCommentTrivia,
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
    BreakKeyword,
    ContinueKeyword,
    ReturnKeyword,


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
    BreakStatement,
    ContinueStatement,
    ReturnStatement,

    // Expression
    LiteralExpression,
    NameExpression,
    BinaryExpression,
    UnaryExpression,
    ParenthesizedExpression,
    AssignmentExpression,
    CallExpression,
}
