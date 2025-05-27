using System;
using System.Collections.Generic;
using System.Text;
using System.Linq; // Добавляем using для Linq

namespace Lab1_compile
{
    public class SqlParser
    {
        private List<SqlToken> _tokens;
        private int _currentTokenIndex;
        private List<SqlParseError> _errors; // Используем новый тип ошибки
        private List<string> _parseSteps;

        public SqlParser(List<SqlToken> tokens)
        {
            _tokens = tokens;
            _currentTokenIndex = 0;
            _errors = new List<SqlParseError>();
            _parseSteps = new List<string>();
        }

        private SqlToken CurrentToken => _tokens[_currentTokenIndex];

        // Метод для перехода к следующему токену
        private void Consume(SqlTokenType expectedType)
        {
            if (CurrentToken.Type == expectedType)
            {
                _currentTokenIndex++;
            }
            else
            {
                // Ошибка: ожидался один тип токена, найден другой
                AddError($"Ошибка синтаксического анализа: ожидался токен типа {expectedType}, найден '{CurrentToken.Value}' ({CurrentToken.Type}) в позиции {CurrentToken.Position}", CurrentToken);
                // В режиме восстановления продвигаемся к следующему токену
                if (_currentTokenIndex < _tokens.Count - 1)
                {
                    _currentTokenIndex++;
                }
            }
        }

        // Метод для добавления ошибки с передачей токена
        private void AddError(string message, SqlToken token)
        {
             _errors.Add(new SqlParseError(message, token));
        }

        // Простая функция восстановления: пропускает токены до одного из recoveryTokens или EndOfInput
        private void Synchronize(params SqlTokenType[] synchronizationTokens)
        {
             // Добавляем EndOfInput как общий токен синхронизации
            var syncTokens = synchronizationTokens.ToList();
            if (!syncTokens.Contains(SqlTokenType.EndOfInput))
            {
                syncTokens.Add(SqlTokenType.EndOfInput);
            }

            // Пропускаем токены до тех пор, пока не найдем синхронизирующий токен или не достигнем конца ввода
            while (_currentTokenIndex < _tokens.Count - 1 && !syncTokens.Contains(CurrentToken.Type))
            {
                _currentTokenIndex++;
            }
        }

        // Функция для нетерминала S (начальное правило) S → select X from X
        // Набор синхронизации для S: EndOfInput
        private bool ParseS(params SqlTokenType[] parentSynchronizationTokens)
        {
            _parseSteps.Add("S");
            var synchronizationTokens = parentSynchronizationTokens.Append(SqlTokenType.EndOfInput).ToArray();

            // Ожидаем ключевое слово 'select'
            if (CurrentToken.Type == SqlTokenType.SelectKeyword)
            {
                Consume(SqlTokenType.SelectKeyword);
            }
            else
            {
                AddError("Ошибка: ожидается ключевое слово 'select'", CurrentToken);
                // В случае ошибки на select, синхронизируемся до from или EndOfInput
                Synchronize(SqlTokenType.FromKeyword, SqlTokenType.EndOfInput);
                // После синхронизации не возвращаем false сразу, попробуем продолжить, если нашли from
                if(CurrentToken.Type != SqlTokenType.FromKeyword)
                    return false; // Не смогли найти from для продолжения
            }

            // Ожидаем нетерминал X (список полей)
            // Набор синхронизации для X после select: Comma, FromKeyword, EndOfInput
            if (!ParseX(SqlTokenType.Comma, SqlTokenType.FromKeyword, SqlTokenType.EndOfInput))
            {
                // Ошибка произошла внутри ParseX, синхронизация уже выполнена там до Comma, From или EndOfInput
                // Продолжаем парсинг S с текущей позиции токена
                if (CurrentToken.Type != SqlTokenType.FromKeyword && 
                    CurrentToken.Type != SqlTokenType.EndOfInput)
                {
                    // Если не удалось синхронизироваться до from или конца ввода,
                    // пытаемся синхронизироваться еще раз
                    Synchronize(SqlTokenType.FromKeyword, SqlTokenType.EndOfInput);
                    if (CurrentToken.Type != SqlTokenType.FromKeyword && 
                        CurrentToken.Type != SqlTokenType.EndOfInput)
                    {
                        return false;
                    }
                }
            }

            // Ожидаем ключевое слово 'from'
            // Если текущий токен не FromKeyword, значит предыдущий ParseX не смог дойти до FromKeyword или EndOfInput
            if (CurrentToken.Type == SqlTokenType.FromKeyword)
            {
                Consume(SqlTokenType.FromKeyword);
            }
            else // Текущий токен не from
            {
                AddError("Ошибка: ожидается ключевое слово 'from'", CurrentToken);
                // В случае ошибки на from, синхронизируемся до EndOfInput
                Synchronize(SqlTokenType.EndOfInput);
                if(CurrentToken.Type != SqlTokenType.EndOfInput)
                    return false; // Не смогли найти EndOfInput после ошибки на from
            }

            // Ожидаем нетерминал X (список таблиц)
            // Набор синхронизации для X после from: EndOfInput
            if (!ParseX(SqlTokenType.EndOfInput))
            {
                // Ошибка произошла внутри ParseX, синхронизация уже выполнена там до EndOfInput
                // Продолжаем парсинг S с текущей позиции токена (ожидая EndOfInput)
                if (CurrentToken.Type != SqlTokenType.EndOfInput)
                {
                    // Если не удалось синхронизироваться до конца ввода,
                    // пытаемся синхронизироваться еще раз
                    Synchronize(SqlTokenType.EndOfInput);
                    if (CurrentToken.Type != SqlTokenType.EndOfInput)
                    {
                        return false;
                    }
                }
            }

            // В идеале, после парсинга S мы должны быть в конце ввода
            if (CurrentToken.Type != SqlTokenType.EndOfInput)
            {
                AddError($"Ошибка: неожиданный токен '{CurrentToken.Value}' после полного разбора запроса", CurrentToken);
                // Синхронизируемся до конца ввода
                Synchronize(SqlTokenType.EndOfInput);
            }

            // S успешен, если мы дошли до конца ввода
            return CurrentToken.Type == SqlTokenType.EndOfInput;
        }

        // Функция для нетерминала X: X → AY
        // Набор синхронизации для X: из родителя + Comma, FromKeyword, EndOfInput
        private bool ParseX(params SqlTokenType[] parentSynchronizationTokens)
        {
            _parseSteps.Add("X");
            var synchronizationTokens = parentSynchronizationTokens.Append(SqlTokenType.Comma).Append(SqlTokenType.FromKeyword).Append(SqlTokenType.EndOfInput).ToArray();

            // Ожидаем нетерминал A
            if (!ParseA(synchronizationTokens.Append(SqlTokenType.Comma).ToArray()))
            {
                // Ошибка произошла внутри ParseA
                AddError($"Ошибка в списке полей/таблиц: не удалось синхронизироваться после ошибки", CurrentToken);
                // Пытаемся синхронизироваться до следующего допустимого токена
                Synchronize(synchronizationTokens);
                // Продолжаем разбор, если не в конце ввода
                return CurrentToken.Type != SqlTokenType.EndOfInput;
            }

            // После ParseA (успешного или с ошибкой), проверяем возможность парсинга Y
            if (CurrentToken.Type == SqlTokenType.Comma || 
                CurrentToken.Type == SqlTokenType.FromKeyword || 
                CurrentToken.Type == SqlTokenType.EndOfInput || 
                parentSynchronizationTokens.Contains(CurrentToken.Type))
            {
                if (!ParseY(synchronizationTokens))
                {
                    // Ошибка произошла внутри ParseY
                    AddError($"Ошибка в продолжении списка: не удалось синхронизироваться после ошибки", CurrentToken);
                    // Пытаемся синхронизироваться до следующего допустимого токена
                    Synchronize(synchronizationTokens);
                    // Продолжаем разбор, если не в конце ввода
                    return CurrentToken.Type != SqlTokenType.EndOfInput;
                }
            }
            else if (CurrentToken.Type != SqlTokenType.EndOfInput)
            {
                // Неожиданный токен после A
                AddError($"Ошибка: неожиданный токен '{CurrentToken.Value}' после идентификатора", CurrentToken);
                // Пытаемся синхронизироваться до следующего допустимого токена
                Synchronize(synchronizationTokens);
                // Продолжаем разбор, если не в конце ввода
                return CurrentToken.Type != SqlTokenType.EndOfInput;
            }

            return synchronizationTokens.Contains(CurrentToken.Type) || 
                   parentSynchronizationTokens.Contains(CurrentToken.Type);
        }

        // Функция для нетерминала Y: Y → ,AY | ɛ
        // Набор синхронизации для Y: из родителя (из X) + Comma, FromKeyword, EndOfInput
        private bool ParseY(params SqlTokenType[] parentSynchronizationTokens)
        {
            _parseSteps.Add("Y");
            var synchronizationTokens = parentSynchronizationTokens.Append(SqlTokenType.Comma).Append(SqlTokenType.FromKeyword).Append(SqlTokenType.EndOfInput).ToArray();

            // Правило Y → ,AY
            if (CurrentToken.Type == SqlTokenType.Comma)
            {
                _parseSteps.Add(",");
                Consume(SqlTokenType.Comma);

                // Ожидаем нетерминал A после запятой
                if (!ParseA(synchronizationTokens.Append(SqlTokenType.Comma).ToArray()))
                {
                    // Ошибка произошла внутри ParseA
                    AddError($"Ошибка в продолжении списка: не удалось синхронизироваться после запятой", CurrentToken);
                    // Пытаемся синхронизироваться до следующего допустимого токена
                    Synchronize(synchronizationTokens);
                    // Продолжаем разбор, если не в конце ввода
                    return CurrentToken.Type != SqlTokenType.EndOfInput;
                }

                // После ParseA (успешного или с ошибкой), проверяем возможность продолжения списка
                if (CurrentToken.Type == SqlTokenType.Comma || 
                    CurrentToken.Type == SqlTokenType.FromKeyword || 
                    CurrentToken.Type == SqlTokenType.EndOfInput || 
                    parentSynchronizationTokens.Contains(CurrentToken.Type))
                {
                    if (!ParseY(synchronizationTokens))
                    {
                        // Ошибка произошла внутри ParseY
                        AddError($"Ошибка в продолжении списка: не удалось синхронизироваться после ошибки", CurrentToken);
                        // Пытаемся синхронизироваться до следующего допустимого токена
                        Synchronize(synchronizationTokens);
                        // Продолжаем разбор, если не в конце ввода
                        return CurrentToken.Type != SqlTokenType.EndOfInput;
                    }
                }
                else if (CurrentToken.Type != SqlTokenType.EndOfInput)
                {
                    // Неожиданный токен после A в списке
                    AddError($"Ошибка: неожиданный токен '{CurrentToken.Value}' после идентификатора в списке", CurrentToken);
                    // Пытаемся синхронизироваться до следующего допустимого токена
                    Synchronize(synchronizationTokens);
                    // Продолжаем разбор, если не в конце ввода
                    return CurrentToken.Type != SqlTokenType.EndOfInput;
                }

                return synchronizationTokens.Contains(CurrentToken.Type) || 
                       parentSynchronizationTokens.Contains(CurrentToken.Type);
            }

            // Правило Y → ɛ (пустая цепочка)
            if (CurrentToken.Type == SqlTokenType.Comma || 
                CurrentToken.Type == SqlTokenType.FromKeyword || 
                CurrentToken.Type == SqlTokenType.EndOfInput || 
                parentSynchronizationTokens.Contains(CurrentToken.Type))
            {
                _parseSteps.Add("ɛ");
                return true;
            }
            else if (CurrentToken.Type != SqlTokenType.Unknown)
            {
                AddError($"Ошибка: неожиданный токен '{CurrentToken.Value}'. Ожидается запятая, 'from' или конец ввода.", CurrentToken);
                // Пытаемся синхронизироваться до следующего допустимого токена
                Synchronize(synchronizationTokens);
                // Продолжаем разбор, если не в конце ввода
                return CurrentToken.Type != SqlTokenType.EndOfInput;
            }
            else
            {
                Synchronize(synchronizationTokens);
                // Продолжаем разбор, если не в конце ввода
                return CurrentToken.Type != SqlTokenType.EndOfInput;
            }
        }

        // Функция для нетерминала A: A → letter {letter}
        // Набор синхронизации для A: из родителя (из X или Y) + Comma, FromKeyword, EndOfInput
        private bool ParseA(params SqlTokenType[] parentSynchronizationTokens)
        {
            _parseSteps.Add("A");
            // Токены синхронизации для A включают токены синхронизации родителя X или Y
            var synchronizationTokens = parentSynchronizationTokens.Append(SqlTokenType.Comma).Append(SqlTokenType.FromKeyword).Append(SqlTokenType.EndOfInput).ToArray();

            // Ожидаем токен Identifier (который соответствует letter {letter})
            if (CurrentToken.Type == SqlTokenType.Identifier)
            {
                _parseSteps.Add(CurrentToken.Value); // Записываем значение идентификатора
                Consume(SqlTokenType.Identifier);
                return true;
            }
            else
            {
                AddError("Ошибка: ожидается идентификатор (столбец или таблица)", CurrentToken);
                // В случае ошибки идентификатора, синхронизируемся до токенов синхронизации A или родителя
                Synchronize(synchronizationTokens);
                
                // Проверяем, удалось ли синхронизироваться до допустимого токена
                if (synchronizationTokens.Contains(CurrentToken.Type))
                {
                    // Если удалось синхронизироваться до допустимого токена, продолжаем разбор
                    return true;
                }
                
                // Если не удалось синхронизироваться, но мы не в конце ввода,
                // все равно продолжаем разбор
                if (CurrentToken.Type != SqlTokenType.EndOfInput)
                {
                    return true;
                }
                
                return false; // Не нашли идентификатор и не смогли синхронизироваться
            }
        }

        // Главный метод парсинга
        public bool Parse()
        {
            _parseSteps.Clear(); // Очищаем предыдущие шаги
            _errors.Clear(); // Очищаем предыдущие ошибки
            _currentTokenIndex = 0; // Сбрасываем индекс токена

            // Начинаем с начального нетерминала S, передавая пустой набор синхронизации для самого верхнего уровня
            bool success = ParseS();

            // После завершения парсинга S, проверяем, находимся ли мы в конце ввода
            if (CurrentToken.Type != SqlTokenType.EndOfInput && !_errors.Any())
            {
                AddError($"Ошибка: неожиданный токен '{CurrentToken.Value}' после полного разбора запроса", CurrentToken);
            }

            // Парсинг считается успешным, если мы дошли до конца ввода, даже если были ошибки
            return CurrentToken.Type == SqlTokenType.EndOfInput;
        }

        // Метод для получения шагов разбора
        public List<string> GetParseSteps()
        {
            return _parseSteps;
        }

        // Метод для получения ошибок
        public List<SqlParseError> GetErrors()
        {
            return _errors;
        }
    }
} 