using System.Collections.Immutable;
using Hyper.Compiler.Parser;
using Hyper.Compiler.Syntax;
using Hyper.Compiler.Syntax.Expr;
using Hyper.Compiler.Syntax.Stmt;

namespace Hyper.Compiler.VM
{
    public sealed class CompilationUnit : Node
    {
        public CompilationUnit(ImmutableArray<MemberSyntax> members, Token endOfFileToken)
        {
            Members = members;
            EndOfFileToken = endOfFileToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

        public ImmutableArray<MemberSyntax> Members        { get; }
        public Token                        EndOfFileToken { get; }
    }
}
