using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1_compile
{
    public class Lexer
    {
        private string _input;
        private int _position;
        private List<ParseError> _errors;

        public Lexer(string input)
        {
            _input = input;
            _position = 0;
            _errors = new List<ParseError>();
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();

            while (_position < _input.Length)
            {
                char currentChar = _input[_position];

                if (char.IsWhiteSpace(currentChar))
                {
                    _position++;
                    continue;
                }

                if (char.IsDigit(currentChar))
                {
                    tokens.Add(ExtractNumber());
                    continue;
                }

                if (currentChar == '-')
                {
                    // Унарный минус: если это первый символ или после ( или после оператора
                    bool isUnary = tokens.Count == 0 ||
                        (tokens.Last().Type == 4 || tokens.Last().Type == 5);

                    if (isUnary && _position + 1 < _input.Length && char.IsDigit(_input[_position + 1]))
                    {
                        tokens.Add(ExtractNumber());
                    }
                    else
                    {
                        tokens.Add(new Token(4, "Оператор", "-", _position, _position));
                        _position++;
                    }
                    continue;
                }

                if (currentChar == '+')
                {
                    tokens.Add(new Token(4, "Оператор", "+", _position, _position));
                    _position++;
                    continue;
                }

                if (currentChar == '*')
                {
                    tokens.Add(new Token(4, "Оператор", "*", _position, _position));
                    _position++;
                    continue;
                }

                if (currentChar == '/')
                {
                    tokens.Add(new Token(4, "Оператор", "/", _position, _position));
                    _position++;
                    continue;
                }

                if (currentChar == '(')
                {
                    tokens.Add(new Token(5, "Скобка", "(", _position, _position));
                    _position++;
                    continue;
                }

                if (currentChar == ')')
                {
                    tokens.Add(new Token(6, "Скобка", ")", _position, _position));
                    _position++;
                    continue;
                }

                _errors.Add(new ParseError($"Ошибка: недопустимый символ '{currentChar}' в позиции {_position}",
                    new Token(-1, "Ошибка", currentChar.ToString(), _position, _position)));

                _position++;
            }

            return tokens;
        }

        private Token ExtractNumber()
        {
            int start = _position;
            bool hasDecimal = false;
            bool isNegative = false;

            if (_input[_position] == '-')
            {
                isNegative = true;
                _position++;
            }

            while (_position < _input.Length && char.IsDigit(_input[_position]))
            {
                _position++;
            }

            if (_position < _input.Length && _input[_position] == '.')
            {
                hasDecimal = true;
                _position++;

                if (_position >= _input.Length || !char.IsDigit(_input[_position]))
                {
                    _errors.Add(new ParseError($"Ошибка: ожидалось число после точки, найдено '{_input.Substring(start, _position - start)}'",
                        new Token(-1, "Ошибка", _input.Substring(start, _position - start), start, _position - 1)));

                    _position++;
                    return null;
                }

                while (_position < _input.Length && char.IsDigit(_input[_position]))
                {
                    _position++;
                }
            }

            string value = _input.Substring(start, _position - start);
            return new Token(hasDecimal ? 9 : 8, hasDecimal ? "Число (вещественное)" : "Число (целое)", value, start, _position - 1);
        }

        public List<ParseError> GetErrors()
        {
            return _errors;
        }
    }
}