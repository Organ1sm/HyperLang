﻿using Hyper.Compiler.Binding;
using Hyper.Compiler.Symbol;

namespace Hyper.Compiler.Parser
{
    internal sealed class Evaluator
    {
        private readonly BoundExpression                    _root;
        private readonly Dictionary<VariableSymbol, object> _variables;

        public Evaluator(BoundExpression root, Dictionary<VariableSymbol, object> variables)
        {
            this._root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression n)
                return n.Value;

            if (node is BoundVariableExpression v)
                return _variables[v.Variable];

            if (node is BoundAssignmentExpression a)
            {
                var value = EvaluateExpression(a.Expression);
                _variables[a.Variable] = value;

                return value;
            }

            if (node is BoundUnaryExpression u)
            {
                var operand = EvaluateExpression(u.Operand);

                return u.Operator.Kind switch
                {
                    BoundUnaryOperatorKind.Identity => (int) operand,
                    BoundUnaryOperatorKind.Negation => -(int) operand,
                    BoundUnaryOperatorKind.LogicalNegation => !(bool) operand,
                    _ => throw new Exception($"Unexpected unary operator {u.Operator}")
                };
            }

            if (node is BoundBinaryExpression b)
            {
                var left  = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                return b.Operator.OpKind switch
                {
                    BoundBinaryOperatorKind.Addition => (int) left + (int) right,
                    BoundBinaryOperatorKind.Subtraction => (int) left - (int) right,
                    BoundBinaryOperatorKind.Multiplication => (int) left * (int) right,
                    BoundBinaryOperatorKind.Division => (int) left / (int) right,
                    BoundBinaryOperatorKind.LogicalAnd => (bool) left && (bool) right,
                    BoundBinaryOperatorKind.LogicalOr => (bool) left || (bool) right,
                    BoundBinaryOperatorKind.Equals => Equals(left, right),
                    BoundBinaryOperatorKind.NotEquals => !Equals(left, right),
                    _ => throw new Exception($"Unexpected binary operator {b.Operator}")
                };
            }

            throw new Exception($"Unexpected node {node.Kind}");
        }
    }
}
