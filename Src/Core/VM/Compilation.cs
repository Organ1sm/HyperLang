﻿using System.Collections.Immutable;
using Hyper.Core.Symbols;
using Hyper.Core.Syntax;
using Hyper.Core.Binding;
using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Scope;

namespace Hyper.Core.VM
{
    public sealed class Compilation
    {
        public  AST               Ast      { get; }
        public  Compilation?      Previous { get; }
        private BoundGlobalScope? _globalScope;

        public Compilation(AST ast) : this(null, ast) { }

        private Compilation(Compilation? previous, AST ast)
        {
            Previous = previous;
            Ast = ast;
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var diagnostics = Ast.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);

            var program = Binder.BindProgram(GlobalScope);
            if (program.Diagnostics.Any())
                return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);

            var evaluator = new Evaluator(program, variables);
            var value     = evaluator.Evaluate();

            return new EvaluationResult(ImmutableArray<Diagnostic.Diagnostic>.Empty, value);
        }

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, Ast.Root);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }

        public Compilation ContinueWith(AST ast)
        {
            return new Compilation(this, ast);
        }

        public void EmitTree(TextWriter writer)
        {
            var program = Binder.BindProgram(GlobalScope);
            program.Statements.WriteTo(writer);
        }
    }
}
