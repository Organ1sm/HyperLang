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
