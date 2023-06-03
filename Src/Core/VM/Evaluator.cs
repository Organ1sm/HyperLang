using System.Diagnostics;
using Hyper.Core.Symbols;
using Hyper.Core.Binding;
using Hyper.Core.Binding.Expr;
using Hyper.Core.Binding.Operator;
using Hyper.Core.Binding.Stmt;

namespace Hyper.Core.VM
{
    internal sealed class Evaluator
    {
        private readonly BoundProgram                                    _program;
        private readonly Dictionary<VariableSymbol, object>              _globals;
        private readonly Dictionary<FunctionSymbol, BoundBlockStatement> _functions = new();
        private readonly Stack<Dictionary<VariableSymbol, object>>       _locals    = new();
        private          Random?                                         _random;

        private object? _lastValue;

        public Evaluator(BoundProgram program, Dictionary<VariableSymbol, object> variables)
        {
            _program = program;
            _globals = variables;
            _locals.Push(new());

            var current = program;
            while (current != null)
            {
                foreach (var kv in current.Functions)
                {
                    var function = kv.Key;
                    var body     = kv.Value;

                    _functions.Add(function, body);
                }

                current = current.Previous;
            }
        }

        public object? Evaluate()
        {
            var function = _program.MainFunction ?? _program.ScriptFunction;
            if (function == null)
                return null;

            var body = _functions[function];
            return EvaluateStatement(body);
        }

        private object? EvaluateStatement(BoundBlockStatement body)
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();
            for (var i = 0; i < body.Statements.Length; i++)
            {
                if (body.Statements[i] is BoundLabelStatement l)
                    labelToIndex.Add(l.Label, i + 1);
            }

            var index = 0;

            while (index < body.Statements.Length)
            {
                var s = body.Statements[index];
                switch (s.Kind)
                {
                    case BoundNodeKind.NopStatement:
                        index++;
                        break;
                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement((BoundExpressionStatement) s);
                        index++;
                        break;
                    case BoundNodeKind.VariableDeclaration:
                        EvaluateVariableDeclaration((BoundVariableDeclaration) s);
                        index++;
                        break;
                    case BoundNodeKind.GotoStatement:
                        var gs = (BoundGotoStatement) s;
                        index = labelToIndex[gs.Label];
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                    {
                        var cgs       = (BoundConditionalGotoStatement) s;
                        var condition = (bool) EvaluateExpression(cgs.Condition)!;

                        if (condition == cgs.JumpIfTrue)
                            index = labelToIndex[cgs.Label];
                        else
                            index++;

                        break;
                    }
                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;

                    case BoundNodeKind.ReturnStatement:
                        var rs = (BoundReturnStatement) s;
                        _lastValue = rs.Expression == null ? null : EvaluateExpression(rs.Expression);
                        return _lastValue;

                    default:
                        throw new Exception($"Unexpected s {s.Kind}");
                }
            }

            return _lastValue;
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            Debug.Assert(value != null);

            _lastValue = value;
            Assign(node.Variable, value);
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            _lastValue = EvaluateExpression(node.Expression);
        }

        private static object EvaluateConstantExpression(BoundExpression n) => n.ConstantValue.Value;

        private object EvaluateVariableExpression(BoundVariableExpression v)
        {
            if (v.Variable.Kind == SymbolKind.GlobalVariable)
                return _globals[v.Variable];

            var locals = _locals.Peek();
            return locals[v.Variable];
        }

        private object EvaluateAssignmentExpression(BoundAssignmentExpression a)
        {
            var value = EvaluateExpression(a.Expression);
            Debug.Assert(value != null);
            Assign(a.Variable, value);

            return value;
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression u)
        {
            var operand = EvaluateExpression(u.Operand);
            Debug.Assert(operand != null);

            return u.Operator.OpKind switch
            {
                BoundUnaryOperatorKind.Identity        => (int) operand,
                BoundUnaryOperatorKind.Negation        => -(int) operand,
                BoundUnaryOperatorKind.LogicalNegation => !(bool) operand,
                BoundUnaryOperatorKind.OnesComplement  => ~(int) operand,

                _ => throw new Exception($"Unexpected unary operator {u.Operator}")
            };
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            var left  = EvaluateExpression(b.Left);
            var right = EvaluateExpression(b.Right);

            Debug.Assert(left != null && right != null);

            return b.Operator.OpKind switch
            {
                BoundBinaryOperatorKind.Addition when b.Type == TypeSymbol.Int    => (int) left + (int) right,
                BoundBinaryOperatorKind.Addition when b.Type == TypeSymbol.String => (string) left + (string) right,

                BoundBinaryOperatorKind.Subtraction    => (int) left - (int) right,
                BoundBinaryOperatorKind.Multiplication => (int) left * (int) right,
                BoundBinaryOperatorKind.Division       => (int) left / (int) right,

                BoundBinaryOperatorKind.BitwiseAnd when b.Type == TypeSymbol.Int  => (int) left & (int) right,
                BoundBinaryOperatorKind.BitwiseAnd when b.Type == TypeSymbol.Bool => (bool) left & (bool) right,

                BoundBinaryOperatorKind.BitwiseOr when b.Type == TypeSymbol.Int  => (int) left | (int) right,
                BoundBinaryOperatorKind.BitwiseOr when b.Type == TypeSymbol.Bool => (bool) left | (bool) right,

                BoundBinaryOperatorKind.BitwiseXor when b.Type == TypeSymbol.Int  => (int) left ^ (int) right,
                BoundBinaryOperatorKind.BitwiseXor when b.Type == TypeSymbol.Bool => (bool) left ^ (bool) right,

                BoundBinaryOperatorKind.LogicalAnd      => (bool) left && (bool) right,
                BoundBinaryOperatorKind.LogicalOr       => (bool) left || (bool) right,
                BoundBinaryOperatorKind.Equals          => Equals(left, right),
                BoundBinaryOperatorKind.NotEquals       => !Equals(left, right),
                BoundBinaryOperatorKind.Less            => (int) left < (int) right,
                BoundBinaryOperatorKind.LessOrEquals    => (int) left <= (int) right,
                BoundBinaryOperatorKind.Greater         => (int) left > (int) right,
                BoundBinaryOperatorKind.GreaterOrEquals => (int) left >= (int) right,

                _ => throw new Exception($"Unexpected binary operator {b.Operator}")
            };
        }

        private object? EvaluateCallExpression(BoundCallExpression node)
        {
            if (node.Function == BuiltinFunctions.Input)
                return Console.ReadLine();

            if (node.Function == BuiltinFunctions.Print)
            {
                var value = EvaluateExpression(node.Arguments[0]);
                Console.WriteLine(value);
                return null;
            }

            if (node.Function == BuiltinFunctions.Rnd)
            {
                var max = (int) EvaluateExpression(node.Arguments[0])!;

                if (_random == null)
                    _random = new Random();

                return _random.Next(max);
            }

            var locals = new Dictionary<VariableSymbol, object>();
            for (int i = 0; i < node.Arguments.Length; i++)
            {
                var parameter = node.Function.Parameters[i];
                var value     = EvaluateExpression(node.Arguments[i]);
                Debug.Assert(value != null);
                locals.Add(parameter, value);
            }

            _locals.Push(locals);

            var statement = _functions[node.Function];
            var result    = EvaluateStatement(statement);

            _locals.Pop();

            return result;
        }

        private object? EvaluateConversionExpression(BoundConversionExpression node)
        {
            var value = EvaluateExpression(node.Expression);

            if (node.Type == TypeSymbol.Any)
                return value;

            if (node.Type == TypeSymbol.Bool)
                return Convert.ToBoolean(value);

            if (node.Type == TypeSymbol.Int)
                return Convert.ToInt32(value);

            if (node.Type == TypeSymbol.String)
                return Convert.ToString(value);

            throw new Exception($"Unexpected type {node.Type}");
        }

        private object? EvaluateExpression(BoundExpression node)
        {
            if (node.ConstantValue != null)
                return EvaluateConstantExpression(node);
            return (node switch
            {
                BoundLiteralExpression n     => EvaluateConstantExpression(n),
                BoundVariableExpression v    => EvaluateVariableExpression(v),
                BoundAssignmentExpression a  => EvaluateAssignmentExpression(a),
                BoundUnaryExpression u       => EvaluateUnaryExpression(u),
                BoundBinaryExpression b      => EvaluateBinaryExpression(b),
                BoundCallExpression c        => EvaluateCallExpression(c),
                BoundConversionExpression cv => EvaluateConversionExpression(cv),
                _                            => throw new Exception($"Unexpected node {node.Kind}")
            })!;
        }

        private void Assign(VariableSymbol variable, object value)
        {
            if (variable.Kind == SymbolKind.GlobalVariable)
                _globals[variable] = value;
            else
            {
                var locals = _locals.Peek();
                locals[variable] = value;
            }
        }
    }
}
