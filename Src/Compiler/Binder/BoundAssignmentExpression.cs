﻿using Hyper.Compiler.Symbols;
using Hyper.Compiler.Syntax;
using Expression = System.Linq.Expressions.Expression;

namespace Hyper.Compiler.Binding;

internal class BoundAssignmentExpression : BoundExpression
{
    public BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression)
    {
        Expression = expression;
        Variable = variable;
    }

    public override BoundNodeKind   Kind       => BoundNodeKind.AssignmentExpression;
    public override TypeSymbol      Type       => Expression.Type;
    public          VariableSymbol  Variable   { get; }
    public          BoundExpression Expression { get; }
}
