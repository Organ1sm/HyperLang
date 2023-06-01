using System.Collections.Immutable;
using Hyper.Core.Syntax;
using Hyper.Core.Syntax.Expr;
using Hyper.Core.Parser;
using Hyper.Core.Syntax.Stmt;

namespace Hyper.Core.VM
{
    public sealed class CompilationUnit : Node
    {
        public CompilationUnit(AST syntaxTree, ImmutableArray<MemberSyntax> members, Token endOfFileToken)
            : base(syntaxTree)
        {
            Members = members;
            EndOfFileToken = endOfFileToken;
        }

        public override IEnumerable<Node> GetChildren()
        {
            foreach (var child in Members)
                yield return child;
            
            yield return EndOfFileToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

        public ImmutableArray<MemberSyntax> Members        { get; }
        public Token                        EndOfFileToken { get; }
    }
}
