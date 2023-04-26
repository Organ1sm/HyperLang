using System.Collections.Immutable;
using Hyper.Core.Binding;
using Hyper.Core.Symbols;
using Hyper.Core.Syntax;
using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Opt;
using Hyper.Core.Binding.Scope;
using Hyper.Core.Binding.Stmt;
using Binder = Hyper.Core.Binding.Binder;
using ReflectionBindingFlags = System.Reflection.BindingFlags;

namespace Hyper.Core.VM
{
    public sealed class Compilation
    {
        public  ImmutableArray<AST> SyntaxTrees;
        public  Compilation?        Previous { get; }
        private BoundGlobalScope?   _globalScope;

        public ImmutableArray<FunctionSymbol>? Functions => GlobalScope.Functions;
        public ImmutableArray<VariableSymbol>? Variables => GlobalScope.Variables;

        public Compilation(params AST[] syntaxTrees) : this(null, syntaxTrees) { }

        private Compilation(Compilation? previous, params AST[] syntaxTrees)
        {
            Previous = previous;
            SyntaxTrees = syntaxTrees.ToImmutableArray();
        }

        private BoundProgram GetProgram()
        {
            var previous = Previous?.GetProgram() ?? null;
            return Binder.BindProgram(previous, GlobalScope);
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);

            var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);

            var program = GetProgram();

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

        public IEnumerable<Symbol> GetSymbols()
        {
            var submission      = this;
            var seenSymbolNames = new HashSet<string>();

            while (submission != null)
            {
                const ReflectionBindingFlags bindingFlags = ReflectionBindingFlags.Static |
                                                            ReflectionBindingFlags.Public |
                                                            ReflectionBindingFlags.NonPublic;

                var builtinFunctions = typeof(BuiltinFunctions)
                                      .GetFields(bindingFlags)
                                      .Where(fi => fi.FieldType == typeof(FunctionSymbol))
                                      .Select(fi => (FunctionSymbol) fi.GetValue(null))
                                      .ToList();

                foreach (var bf in builtinFunctions)
                {
                    if (seenSymbolNames.Add(bf.Name))
                        yield return bf;
                }

                foreach (var function in submission.Functions)
                {
                    if (seenSymbolNames.Add(function.Name))
                        yield return function;
                }

                foreach (var variable in submission.Variables)
                {
                    if (seenSymbolNames.Add(variable.Name))
                        yield return variable;
                }

                submission = submission.Previous;
            }
        }

        public void EmitTree(TextWriter writer)
        {
            var program = GetProgram();
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

        public void EmitTree(FunctionSymbol symbol, TextWriter writer)
        {
            var program = GetProgram();

            symbol.WriteTo(writer);
            writer.WriteLine();

            if (!program.Functions.TryGetValue(symbol, out var body))
                return;

            body.WriteTo(writer);
        }
    }
}
