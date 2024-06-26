﻿using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public abstract class Statement : Node
{
    protected Statement(AST syntaxTree)
        : base(syntaxTree) { }
}
