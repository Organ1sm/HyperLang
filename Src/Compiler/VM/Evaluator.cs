﻿using Hyper.Compiler.Binding;
using Hyper.Compiler.Symbol;

namespace Hyper.Compiler.VM
{
    internal sealed class Evaluator
    {
        private readonly BoundStatement                     _root;
        private readonly Dictionary<VariableSymbol, object> _variables;

        private object _lastValue;

        public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables)
        {
            this._root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            EvaluateStatement(_root);
            return _lastValue;
        }

        private void EvaluateStatement(BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    EvaluateBlockStatement((BoundBlockStatement) node);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement) node);
                    break;
                default:
                    throw new Exception($"Unexpected node {node.Kind}");
            }
        }

        private void EvaluateBlockStatement(BoundBlockStatement node)
        {
            foreach (var statement in node.Statements)
                EvaluateStatement(statement);
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            _lastValue = EvaluateExpression(node.Expression);
        }

        private static object EvaluateLiteralExpression(BoundLiteralExpression n) => n.Value;
        private object EvaluateVariableExpression(BoundVariableExpression v) => _variables[v.Variable];

        private object EvaluateAssignmentExpression(BoundAssignmentExpression a)
        {
            var value = EvaluateExpression(a.Expression);
            _variables[a.Variable] = value;

            return value;
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression u)
        {
            var operand = EvaluateExpression(u.Operand);

            return u.Operator.Kind switch
            {
                BoundUnaryOperatorKind.Identity        => (int) operand,
                BoundUnaryOperatorKind.Negation        => -(int) operand,
                BoundUnaryOperatorKind.LogicalNegation => !(bool) operand,
                _                                      => throw new Exception($"Unexpected unary operator {u.Operator}")
            };
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression b)
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

        private object EvaluateExpression(BoundExpression node)
        {
            return node switch
            {
                BoundLiteralExpression n    => EvaluateLiteralExpression(n),
                BoundVariableExpression v   => EvaluateVariableExpression(v),
                BoundAssignmentExpression a => EvaluateAssignmentExpression(a),
                BoundUnaryExpression u      => EvaluateUnaryExpression(u),
                BoundBinaryExpression b     => EvaluateBinaryExpression(b),
                _                           => throw new Exception($"Unexpected node {node.Kind}")
            };
        }
    }
}
