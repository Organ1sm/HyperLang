﻿namespace Hyper.Core.Syntax.Expr;

public abstract class Expression : Node
{
    protected Expression(AST syntaxTree)
        : base(syntaxTree) { }
}
