namespace Hyper.Core.Binding.Operator;

internal enum BoundUnaryOperatorKind
{
    Identity,        // +a 
    Negation,        // -a
    LogicalNegation, // !a
    OnesComplement,  // ~a
}
