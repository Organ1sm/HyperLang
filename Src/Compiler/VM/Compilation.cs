using Hyper.Compiler.Binding;
using Hyper.Compiler.Syntax;

namespace Hyper.Compiler.Parser
{
    public sealed class Compilation
    {
        public AST Ast { get; }

        public Compilation(AST ast)
        {
            Ast = ast;
        }

        public EvaluationResult Evaluate()
        {
            var binder          = new Binder();
            var boundExpression = binder.BindExpression(Ast.Root);

            var diagnostics = Ast.Diagnostics.Concat(binder.Diagnostics).ToArray();
            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);

            var evaluator = new Evaluator(boundExpression);
            var value     = evaluator.Evaluate();

            return new EvaluationResult(Array.Empty<Diagnostic.Diagnostic>(), value);
        }
    }
}
