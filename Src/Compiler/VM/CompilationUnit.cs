using Hyper.Compiler.Parser;
using Hyper.Compiler.Syntax;

namespace Hyper.Compiler.VM
{
    public sealed class CompilationUnit : Node
    {
        public CompilationUnit(Expression expression, Token endOfFileToken)
        {
            Expression = expression;
            EndOfFileToken = endOfFileToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

        public Expression Expression     { get; }
        public Token      EndOfFileToken { get; }
    }
}
