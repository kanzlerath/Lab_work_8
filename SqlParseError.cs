using System;

namespace Lab1_compile
{
    // Класс ошибки для SQL парсера
    public class SqlParseError
    {
        public string Message { get; }
        public SqlToken Token { get; }

        public SqlParseError(string message, SqlToken token)
        {
            Message = message;
            Token = token;
        }

        public override string ToString()
        {
            return $"Ошибка: {Message} в позиции {Token.Position}";
        }
    }
} 