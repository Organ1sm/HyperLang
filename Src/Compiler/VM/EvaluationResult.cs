namespace Hyper.Compiler.Parser
{
    public sealed class EvaluationResult
    {
        public EvaluationResult(IEnumerable<Diagnostic.Diagnostic> diagnostics, object value)
        {
            Diagnostics = diagnostics.ToArray();
            Value = value;
        }

        public IReadOnlyList<Diagnostic.Diagnostic> Diagnostics { get; }
        public object                               Value       { get; }
    }
}
