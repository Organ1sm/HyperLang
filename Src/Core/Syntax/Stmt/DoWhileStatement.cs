﻿using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class DoWhileStatement : Statement
{
    public DoWhileStatement(AST syntaxTree, Token doKeyword, Statement body, Token whileKeyword, Expression condition)
        : base(syntaxTree)
    {
        DoKeyword = doKeyword;
        Body = body;
        WhileKeyword = whileKeyword;
        Condition = condition;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return DoKeyword;
        yield return Body;
        yield return WhileKeyword;
        yield return Condition;
    }

    public override SyntaxKind Kind         => SyntaxKind.DoWhileStatement;
    public          Token      DoKeyword    { get; }
    public          Statement  Body         { get; }
    public          Token      WhileKeyword { get; }
    public          Expression Condition    { get; }
}
