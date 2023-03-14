using System.Collections.Immutable;

namespace Hyper.Compiler.Parser
{
    public sealed class EvaluationResult
    {
        public EvaluationResult(ImmutableArray<Diagnostic.Diagnostic> diagnostics, object value)
        {
            Diagnostics = diagnostics;
            Value = value;
        }

        public ImmutableArray<Diagnostic.Diagnostic> Diagnostics { get; }
        public object                                Value       { get; }
    }
}
