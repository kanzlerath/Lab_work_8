using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1_compile
{
    public class PolishNotation
    {
        private List<string> _output = new List<string>();
        private Stack<string> _operators = new Stack<string>();

        public List<string> ConvertToPolishNotation(List<Token> tokens)
        {
            _output.Clear();
            _operators.Clear();

            foreach (var token in tokens)
            {
                if (token == null) continue;
                switch (token.Type)
                {
                    case 8: // Целое число
                    case 9: // Вещественное число
                        _output.Add(token.Value);
                        break;
                    case 4: // Оператор
                        while (_operators.Count > 0 && GetPriority(_operators.Peek()) >= GetPriority(token.Value))
                        {
                            _output.Add(_operators.Pop());
                        }
                        _operators.Push(token.Value);
                        break;
                    case 5: // (
                        _operators.Push(token.Value);
                        break;
                    case 6: // )
                        while (_operators.Count > 0 && _operators.Peek() != "(")
                        {
                            _output.Add(_operators.Pop());
                        }
                        if (_operators.Count > 0 && _operators.Peek() == "(")
                            _operators.Pop();
                        break;
                }
            }

            while (_operators.Count > 0)
            {
                _output.Add(_operators.Pop());
            }

            return new List<string>(_output);
        }

        private int GetPriority(string op)
        {
            switch (op)
            {
                case "+":
                case "-":
                    return 1;
                case "*":
                case "/":
                    return 2;
                default:
                    return 0;
            }
        }

        public double EvaluatePolishNotation(List<string> polish)
        {
            Stack<double> stack = new Stack<double>();
            foreach (var token in polish)
            {
                double num;
                if (double.TryParse(token, out num))
                {
                    stack.Push(num);
                }
                else if (token == "+" || token == "-" || token == "*" || token == "/")
                {
                    if (stack.Count < 2)
                        throw new InvalidOperationException("Недостаточно операндов для операции " + token);
                    double b = stack.Pop();
                    double a = stack.Pop();
                    switch (token)
                    {
                        case "+": stack.Push(a + b); break;
                        case "-": stack.Push(a - b); break;
                        case "*": stack.Push(a * b); break;
                        case "/": stack.Push(a / b); break;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Неизвестный токен в ПОЛИЗ: {token}");
                }
            }
            if (stack.Count != 1)
                throw new InvalidOperationException("Ошибка вычисления ПОЛИЗ: в стеке осталось несколько значений");
            return stack.Pop();
        }

        public List<PolishStep> EvaluatePolishNotationWithSteps(List<string> polish)
        {
            var steps = new List<PolishStep>();
            Stack<double> stack = new Stack<double>();
            foreach (var token in polish)
            {
                string action = "";
                double num;
                if (double.TryParse(token, out num))
                {
                    stack.Push(num);
                    action = $"Положили {num}";
                }
                else if (token == "+" || token == "-" || token == "*" || token == "/")
                {
                    double b = stack.Pop();
                    double a = stack.Pop();
                    double res = 0;
                    switch (token)
                    {
                        case "+": res = a + b; break;
                        case "-": res = a - b; break;
                        case "*": res = a * b; break;
                        case "/": res = a / b; break;
                    }
                    stack.Push(res);
                    action = $"Выполнили {a} {token} {b} = {res}";
                }
                else
                {
                    action = $"Неизвестный токен: {token}";
                }
                steps.Add(new PolishStep
                {
                    Token = token,
                    StackState = string.Join(", ", stack.ToArray().Reverse()),
                    Action = action
                });
            }
            return steps;
        }
    }

    public class PolishStep
    {
        public string Token { get; set; }
        public string StackState { get; set; }
        public string Action { get; set; }
    }
} 