﻿using System.Collections.Immutable;
using Hyper.Core.Binding;
using Hyper.Core.Symbols;
using Hyper.Core.Syntax;
using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Scope;
using Hyper.Core.Emit;

namespace Hyper.Core.VM
{
    public sealed class Compilation
    {
        public  ImmutableArray<AST> SyntaxTrees;
        public  Compilation?        Previous { get; }
        private BoundGlobalScope?   _globalScope;
        private bool                IsScript { get; }

        public FunctionSymbol?                MainFunction => GlobalScope.MainFunction;
        public ImmutableArray<FunctionSymbol> Functions    => GlobalScope.Functions;
        public ImmutableArray<VariableSymbol> Variables    => GlobalScope.Variables;

        private Compilation(bool isScript, Compilation? previous, params AST[] syntaxTrees)
        {
            IsScript = isScript;
            Previous = previous;
            SyntaxTrees = syntaxTrees.ToImmutableArray();
        }

        public static Compilation Create(params AST[] syntaxTrees) => new(isScript: false, previous: null, syntaxTrees);

        public static Compilation CreateScript(Compilation? previous, params AST[] syntaxTrees) =>
            new(isScript: true, previous, syntaxTrees);

        private BoundProgram GetProgram()
        {
            var previous = Previous?.GetProgram() ?? null;
            return Binding.Binder.BindProgram(IsScript, previous, GlobalScope);
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            if (GlobalScope.Diagnostics.Any())
                return new EvaluationResult(GlobalScope.Diagnostics, null);

            var program = GetProgram();

            // var appPath      = Environment.GetCommandLineArgs()[0];
            // var appDirectory = Path.GetDirectoryName(appPath);
            // var cfgPath      = Path.Combine(appDirectory, "cfg.dot");
            //
            // BoundBlockStatement cfgStatement;
            // if (!program.BlockStatement.Statements.Any() && program.Functions.Any())
            //     cfgStatement = program.Functions.Last().Value;
            // else
            //     cfgStatement = program.BlockStatement;
            //
            // var cfg = ControlFlowGraph.Create(cfgStatement);
            // using (var streamWriter = new StreamWriter(cfgPath))
            //     cfg.WriteTo(streamWriter);

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
                    var globalScope = Binding.Binder.BindGlobalScope(IsScript, Previous?.GlobalScope, SyntaxTrees);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }

        public IEnumerable<Symbol> GetSymbols()
        {
            var submission      = this;
            var seenSymbolNames = new HashSet<string>();

            var builtinFunctions = BuiltinFunctions.GetAll().ToList();

            while (submission != null)
            {
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

                foreach (var bf in builtinFunctions)
                {
                    if (seenSymbolNames.Add(bf.Name))
                        yield return bf;
                }

                submission = submission.Previous;
            }
        }

        public void EmitTree(TextWriter writer)
        {
            if (GlobalScope.MainFunction != null)
                EmitTree(GlobalScope.MainFunction, writer);
            else if (GlobalScope.ScriptFunction != null)
                EmitTree(GlobalScope.ScriptFunction, writer);
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

        public ImmutableArray<Diagnostic.Diagnostic> Emit(string moduleName, string[] references, string outputPath)
        {
            var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);
            var diagnostics      = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
                return diagnostics;

            var program = GetProgram();
            return Emitter.Emit(program, moduleName, references, outputPath);
        }
    }
}
