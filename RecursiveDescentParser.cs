using System;
using System.Collections.Generic;

namespace Lab1_compile
{
    public class RecursiveDescentParser
    {
        private List<Token> _tokens;
        private int _currentPosition;
        private List<ParseError> _errors;

        public RecursiveDescentParser(List<Token> tokens)
        {
            _tokens = tokens;
            _currentPosition = 0;
            _errors = new List<ParseError>();
        }

        public bool Parse()
        {
            try
            {
                E();
                return _currentPosition == _tokens.Count && _errors.Count == 0;
            }
            catch (Exception ex)
            {
                _errors.Add(new ParseError($"Ошибка при разборе: {ex.Message}",
                    new Token(-1, "Ошибка", "", _currentPosition, _currentPosition)));
                return false;
            }
        }

        private void E()
        {
            T();
            A();
        }

        private void A()
        {
            if (_currentPosition < _tokens.Count)
            {
                var token = _tokens[_currentPosition];
                if (token.Value == "+" || token.Value == "-")
                {
                    _currentPosition++;
                    T();
                    A();
                }
            }
        }

        private void T()
        {
            O();
            B();
        }

        private void B()
        {
            if (_currentPosition < _tokens.Count)
            {
                var token = _tokens[_currentPosition];
                if (token.Value == "*" || token.Value == "/")
                {
                    _currentPosition++;
                    O();
                    B();
                }
            }
        }

        private void O()
        {
            if (_currentPosition >= _tokens.Count)
            {
                throw new Exception("Ожидалось число или выражение в скобках");
            }

            var token = _tokens[_currentPosition];
            if (token.Type == 8 || token.Type == 9) // Число
            {
                _currentPosition++;
            }
            else if (token.Value == "(")
            {
                _currentPosition++;
                E();
                if (_currentPosition >= _tokens.Count || _tokens[_currentPosition].Value != ")")
                {
                    throw new Exception("Ожидалась закрывающая скобка");
                }
                _currentPosition++;
            }
            else
            {
                throw new Exception($"Ожидалось число или открывающая скобка, получено: {token.Value}");
            }
        }

        public List<ParseError> GetErrors()
        {
            return _errors;
        }
    }
}