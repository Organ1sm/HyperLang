﻿using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax.Stmt;

public class ElseClause : Node
{
    public ElseClause(Token elseKeyword, Statement elseStatement)
    {
        ElseKeyword = elseKeyword;
        ElseStatement = elseStatement;
    }

    public override SyntaxKind Kind          => SyntaxKind.ElseClause;
    public          Token      ElseKeyword   { get; }
    public          Statement  ElseStatement { get; }
}