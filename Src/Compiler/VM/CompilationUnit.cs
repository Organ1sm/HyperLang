using Hyper.Compiler.Parser;
using Hyper.Compiler.Syntax;
using Hyper.Compiler.Syntax.Stmt;

namespace Hyper.Compiler.VM
{
    public sealed class CompilationUnit : Node
    {
        public CompilationUnit(Statement statement, Token endOfFileToken)
        {
            Statement = statement;
            EndOfFileToken = endOfFileToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

        public Statement Statement      { get; }
        public Token     EndOfFileToken { get; }
    }
}
