using System.Collections.Immutable;
using Hyper.Core.Parser;
using Hyper.Core.Syntax.Expr;

namespace Hyper.Core.Syntax.Stmt;

public sealed class BlockStatement : Statement
{
    public BlockStatement(AST syntaxTree,
                          Token openBraceToken,
                          ImmutableArray<Statement> statements,
                          Token closeBraceToken)
        : base(syntaxTree)
    {
        OpenBraceToken = openBraceToken;
        Statements = statements;
        CloseBraceToken = closeBraceToken;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return OpenBraceToken;
        foreach (var child in Statements)
            yield return child;
        yield return CloseBraceToken;
    }

    public override SyntaxKind                Kind            => SyntaxKind.BlockStatement;
    public          Token                     OpenBraceToken  { get; }
    public          ImmutableArray<Statement> Statements      { get; }
    public          Token                     CloseBraceToken { get; }
}
