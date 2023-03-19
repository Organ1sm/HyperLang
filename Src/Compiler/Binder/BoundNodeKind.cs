namespace Hyper.Compiler.Binding;

internal enum BoundNodeKind
{
    // Statements
    BlockStatement,
    ExpressionStatement,
    IfStatement,
    WhileStatement,
    ForStatement, 
    VariableDeclaration,
    
    // Expressions
    LiteralExpression,
    VariableExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
}
