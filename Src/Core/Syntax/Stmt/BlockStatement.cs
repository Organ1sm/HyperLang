using System.Collections.Immutable;
using Hyper.Core.Parser;

namespace Hyper.Core.Syntax.Stmt;

public sealed partial class BlockStatement : Statement
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

    public override SyntaxKind                Kind            => SyntaxKind.BlockStatement;
    public          Token                     OpenBraceToken  { get; }
    public          ImmutableArray<Statement> Statements      { get; }
    public          Token                     CloseBraceToken { get; }
}
