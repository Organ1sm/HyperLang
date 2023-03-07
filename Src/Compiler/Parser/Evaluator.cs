using Hyper.Compiler.Syntax;

namespace Hyper.Compiler.Parser
{
    public sealed class Evaluator
    {
        private readonly Expression _root;

        public Evaluator(Expression root)
        {
            this._root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(Expression node)
        {
            if (node is LiteralExpression n)
                return (int) n.LiteralToken.Value;


            if (node is UnaryExpression u)
            {
                var operand = EvaluateExpression(u.Operand);

                if (u.OperatorToken.Kind == SyntaxKind.PlusToken)
                    return operand;
                else if (u.OperatorToken.Kind == SyntaxKind.MinusToken)
                    return -operand;
                else
                    throw new Exception($"Unexpected unary operator {u.OperatorToken.Kind}");
            }

            if (node is BinaryExpression b)
            {
                var left  = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                if (b.Operator.Kind == SyntaxKind.PlusToken)
                    return left + right;
                else if (b.Operator.Kind == SyntaxKind.MinusToken)
                    return left - right;
                else if (b.Operator.Kind == SyntaxKind.StarToken)
                    return left * right;
                else if (b.Operator.Kind == SyntaxKind.SlashToken)
                    return left / right;
                else
                    throw new Exception($"Unexpected binary operator {b.Operator.Kind}");
            }

            if (node is ParenthesizedExpression p)
                return EvaluateExpression(p.Expression);

            throw new Exception($"Unexpected node {node.Kind}");
        }
    }
}
