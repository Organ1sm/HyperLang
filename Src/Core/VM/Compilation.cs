using System.Collections.Immutable;
using Hyper.Core.Symbols;
using Hyper.Core.Syntax;
using Hyper.Core.Binding;
using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Opt;
using Hyper.Core.Binding.Scope;
using Hyper.Core.Binding.Stmt;

namespace Hyper.Core.VM
{
    public sealed class Compilation
    {
        public  ImmutableArray<AST> SyntaxTrees;
        public  Compilation?        Previous { get; }
        private BoundGlobalScope?   _globalScope;

        public Compilation(params AST[] syntaxTrees) : this(null, syntaxTrees) { }

        private Compilation(Compilation? previous, params AST[] syntaxTrees)
        {
            Previous = previous;
            SyntaxTrees = syntaxTrees.ToImmutableArray();
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);

            var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);

            var program = Binder.BindProgram(GlobalScope);

            var appPath      = Environment.GetCommandLineArgs()[0];
            var appDirectory = Path.GetDirectoryName(appPath);
            var cfgPath      = Path.Combine(appDirectory, "cfg.dot");

            BoundBlockStatement cfgStatement;
            if (!program.BlockStatement.Statements.Any() && program.Functions.Any())
                cfgStatement = program.Functions.Last().Value;
            else
                cfgStatement = program.BlockStatement;

            var cfg = ControlFlowGraph.Create(cfgStatement);
            using (var streamWriter = new StreamWriter(cfgPath))
                cfg.WriteTo(streamWriter);

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
                    var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTrees);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }

        public Compilation ContinueWith(AST ast) => new(this, ast);

        public void EmitTree(TextWriter writer)
        {
            var program = Binder.BindProgram(GlobalScope);
            if (program.BlockStatement.Statements.Any())
                program.BlockStatement.WriteTo(writer);
            else
            {
                foreach (var functionBody in program.Functions.Where(functionBody =>
                                                                         GlobalScope.Functions == null ||
                                                                         GlobalScope.Functions
                                                                            .Contains(functionBody.Key)))
                {
                    functionBody.Key.WriteTo(writer);
                    writer.WriteLine();
                    functionBody.Value.WriteTo(writer);
                }
            }
        }
    }
}
