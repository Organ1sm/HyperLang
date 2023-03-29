namespace Hyper.Compiler.Binding;

internal enum BoundNodeKind
{
    // Statements
    BlockStatement,
    ExpressionStatement,
    IfStatement,
    DoWhileStatement,
    WhileStatement,
    ForStatement,
    LabelStatement,
    GotoStatement,
    ConditionalGotoStatement,
    VariableDeclaration,

    // Expressions
    LiteralExpression,
    VariableExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    ConversionExpression,
    CallExpression,
    ErrorExpression,
}
