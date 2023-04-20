namespace Hyper.Core.Binding.Expr;

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
    ReturnStatement,
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
