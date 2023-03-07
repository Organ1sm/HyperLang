using Hyper.Compiler.Parser;

namespace Hyper.Compiler.Syntax;

public sealed class UnaryExpression : Expression
{
    public override SyntaxKind Kind          => SyntaxKind.UnaryExpression;
    public          Token      OperatorToken { get; }
    public          Expression Operand       { get; }

    public UnaryExpression(Token operatorToken, Expression operand)
    {
        OperatorToken = operatorToken;
        Operand = operand;
    }

    public override IEnumerable<Node> GetChildren()
    {
        yield return OperatorToken;
        yield return Operand;
    }
}
