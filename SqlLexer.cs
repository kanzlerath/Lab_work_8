using System;
using System.Collections.Generic;
using System.Text;

namespace Lab1_compile
{
    // Определения типов токенов для SQL грамматики
    public enum SqlTokenType
    {
        SelectKeyword,
        FromKeyword,
        Comma,
        Identifier,
        EndOfInput,
        Unknown
    }

    // Класс, представляющий токен
    public class SqlToken
    {
        public SqlTokenType Type { get; }
        public string Value { get; }
        public int Position { get; }

        public SqlToken(SqlTokenType type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

        public override string ToString()
        {
            return $"{Type}: '{Value}' @ {Position}";
        }
    }

    // Класс Лексера для SQL грамматики
    public class SqlLexer
    {
        private string _input;
        private int _position;
        private List<SqlParseError> _errors;

        public SqlLexer(string input)
        {
            _input = input;
            _position = 0;
            _errors = new List<SqlParseError>();
        }

        public List<SqlToken> Tokenize()
        {
            List<SqlToken> tokens = new List<SqlToken>();

            while (_position < _input.Length)
            {
                char currentChar = _input[_position];

                // Пропускаем пробельные символы
                if (char.IsWhiteSpace(currentChar))
                {
                    _position++;
                    continue;
                }

                // Ключевые слова
                if (char.IsLetter(currentChar))
                {
                    string keywordOrIdentifier = ExtractIdentifier();
                    SqlTokenType type = SqlTokenType.Identifier;

                    if (keywordOrIdentifier.ToLower() == "select")
                    {
                        type = SqlTokenType.SelectKeyword;
                    }
                    else if (keywordOrIdentifier.ToLower() == "from")
                    {
                        type = SqlTokenType.FromKeyword;
                    }
                    // Если не ключевое слово, остается Identifier

                    tokens.Add(new SqlToken(type, keywordOrIdentifier, _position - keywordOrIdentifier.Length));
                    continue;
                }

                // Запятая
                if (currentChar == ',')
                {
                    tokens.Add(new SqlToken(SqlTokenType.Comma, ",", _position));
                    _position++;
                    continue;
                }

                // Неизвестный символ
                _errors.Add(new SqlParseError($"Ошибка лексического анализа: недопустимый символ '{currentChar}'",
                    new SqlToken(SqlTokenType.Unknown, currentChar.ToString(), _position)));

                _position++;
            }

            tokens.Add(new SqlToken(SqlTokenType.EndOfInput, "", _position)); // Добавляем токен конца ввода
            return tokens;
        }

        private string ExtractIdentifier()
        {
            int start = _position;
            while (_position < _input.Length && char.IsLetterOrDigit(_input[_position]))
            {
                _position++;
            }
            return _input.Substring(start, _position - start);
        }

        public List<SqlParseError> GetErrors()
        {
            return _errors;
        }
    }
} 