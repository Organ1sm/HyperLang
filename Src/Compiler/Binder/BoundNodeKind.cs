namespace Hyper.Compiler.Binding;

internal enum BoundNodeKind
{
    // Statements
    BlockStatement,
    ExpressionStatement,
    IfStatement,
    WhileStatement,
    VariableDeclaration,
    
    // Expressions
    LiteralExpression,
    VariableExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
}
