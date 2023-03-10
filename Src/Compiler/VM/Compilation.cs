﻿using Hyper.Compiler.Binding;
using Hyper.Compiler.Symbol;
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

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var binder          = new Binder(variables);
            var boundExpression = binder.BindExpression(Ast.Root);

            var diagnostics = Ast.Diagnostics.Concat(binder.Diagnostics).ToArray();
            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);

            var evaluator = new Evaluator(boundExpression, variables);
            var value     = evaluator.Evaluate();

            return new EvaluationResult(Array.Empty<Diagnostic.Diagnostic>(), value);
        }
    }
}
