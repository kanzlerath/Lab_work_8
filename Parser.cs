//using System;
//using System.Collections.Generic;
//using System.Xml.Schema;

//namespace Lab1_compile
//{
//    internal class Parser
//    {
//        private string id;
//        private int state;
//        private Stroke stroke;
//        private List<ParseError> errors;

//        public List<ParseError> GetErrors()
//        {
//            return errors;
//        }

//        public bool Parse(Stroke c)
//        {
//            stroke = c;
//            state = 1;
//            id = "";
//            errors = new List<ParseError>();

//            while (state != 16)
//            {
//                switch (state)
//                {
//                    case 1:
//                        State1();
//                        break;
//                    case 2:
//                        State2();
//                        break;
//                    case 3:
//                        State3();
//                        break;
//                    case 4:
//                        State4();
//                        break;
//                    case 5:
//                        State5();
//                        break;
//                    case 6:
//                        State6();
//                        break;
//                    case 7:
//                        State7();
//                        break;
//                    case 8:
//                        State8();
//                        break;
//                    case 9:
//                        State9();
//                        break;
//                    case 10:
//                        State10();
//                        break;
//                    case 11:
//                        State11();
//                        break;
//                    case 12:
//                        State12();
//                        break;
//                    case 13:
//                        State13();
//                        break;
//                    case 14:
//                        State14();
//                        break;
//                    case 15:
//                        State15();
//                        break;
//                }
//            }

//            return true;
//        }

//        private void HandleError(string errorMessage, string incorrectFragment, Character c)
//        {
//            errors.Add(new ParseError(errorMessage, incorrectFragment, c.Idx));
//        }

//        private bool tryStop()
//        {
//            char next = stroke.Next().Char;

//            if (next == '\0' || next == ';')
//            {
//                stroke.GetNext();
//                state = 16;
//                return true;
//            }

//            return false;
//        }

//        private void State1()
//        {
//            Character c = stroke.GetNext();
//            if (c.Char == '\n')
//            {
//                state = 1;
//            }
//            else if (IsLetter(c.Char))
//            {
//                state = 2;
//            }
//            else
//            {
//                string remStr = "";
//                Character firstIncorrect = c;

//                while (!IsLetter(stroke.Next().Char))
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                remStr += c.Char;
//                HandleError("\nОжидался идентификатор.", remStr, firstIncorrect);
//            }
//        }

//        private void State2()
//        {
//            Character c = stroke.GetNext();

//            if (c.Char == '=')
//            {
//                state = 3;
//            }
//            else if (IsLetter(c.Char) || IsDigit(c.Char))
//            {
//                state = 2;
//            }
//            else
//            {
//                String remStr = "";
//                Character firstIncorrect = c;

//                while (!IsLetter(stroke.Next().Char) && !IsDigit(stroke.Next().Char) && stroke.Next().Char != '=' /*&& !isSpace(stroke.Next().Char)*/)
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                remStr += c.Char;
//                HandleError("\nНеожиданный символ: '" + firstIncorrect.Str + "'.", remStr, firstIncorrect);
//                //stroke.MoveBack();
//            }
//        }

//        private bool isSpace(char c)
//        {
//            return c == ' ' || c == '\t' || c == '\r';
//        }

//        private void State3()
//        {
//            Character c = stroke.GetNext();

//            if (IsLetter(c.Char))
//            {
//                state = 4;
//                id += c.Char;
//            }
//            else
//            {
//                string remStr = "";
//                Character firstIncorrect = c;

//                while (!IsLetter(stroke.Next().Char))
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                remStr += c.Char;
//                HandleError("\nОжидалось ключевое слово complex.", remStr, firstIncorrect);
//            }
//        }

//        private void State4()
//        {
//            Character c = stroke.GetNext();
//            if (IsLetter(c.Char))
//            {
//                state = 4;
//                id += c.Char;
//            }
//            else
//            {
//                if (c.Char == '(')
//                {
//                    if (!id.Equals("complex"))
//                    {
//                        int errorPosition = c.Idx - id.Length;
//                        HandleError("\nОжидалось ключевое слово complex.", id, new Character(id[0], errorPosition));
//                    }
//                    state = 5;
//                    /*if (!id.Equals("complex"))
//                    {
//                        int errorPosition = c.Idx - id.Length;
//                        HandleError("\nОжидалось ключевое слово complex.", id, new Character(id[0], errorPosition));
//                    }*/
//                }
//                else
//                {
//                    if (!id.Equals("complex"))
//                    {
//                        int errorPosition = c.Idx - id.Length;
//                        HandleError("\nОжидалось ключевое слово complex.", id, new Character(id[0], errorPosition));
//                    }
//                    //HandleError("\nОжидалась (.", null, c);
//                    String remStr = "";
//                    Character firstIncorrect = c;

//                    while (stroke.Next().Char != '(' || stroke.Next().Char != '+' || stroke.Next().Char != '-' || IsDigit(c.Char)/*&& !isSpace(stroke.Next().Char)*/)
//                    {
//                        if (tryStop()) break;
//                        remStr += c.Char;
//                        c = stroke.GetNext();
//                    }
//                    remStr += c.Char;
//                    HandleError("\nНеожиданный символ: '" + firstIncorrect.Str + "'.", remStr, firstIncorrect);
//                    state = 5;
//                    //stroke.MoveBack();
//                }
//            }
            
//            /*else
//            {
//                if (!id.Equals("complex"))
//                {
//                    string remStr = "";
//                    Character firstIncorrect = c;

//                    while (stroke.Next().Char != '(')
//                    {
//                        if (tryStop()) break;
//                        remStr += c.Char;
//                        c = stroke.GetNext();
//                    }
//                    remStr += c.Char;
//                    HandleError("\nОжидалось ключевое слово complex.", remStr, firstIncorrect);
//                }*/
//                /*if (!id.Equals("complex"))
//                {
//                    int errorPosition = c.Idx - id.Length;
//                    HandleError("\nОжидалось ключевое слово complex.", id, new Character(id[0], errorPosition));
//                }
//                state = 5;
//                stroke.MoveBack();
//                */
//        }

//        private void State5()
//        {
//            Character c = stroke.GetNext();

//            if (c.Char == '+' || c.Char == '-')
//            {
//                state = 6;
//            }
//            else if (IsDigit(c.Char)) state = 6;
//            else if (c.Char == ',')
//            {
//                String remStr = "";
//                Character firstIncorrect = c;
//                HandleError("\nОжидалось число.", remStr, firstIncorrect);
//                state = 10;
//            }
//            else
//            {
//                String remStr = "";
//                Character firstIncorrect = c;

//                while (!IsDigit(stroke.Next().Char) && stroke.Next().Char != '+' && stroke.Next().Char != '-' && stroke.Next().Char != ',')
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                if (stroke.Next().Char == ',')
//                {
//                    HandleError("\nОжидалось число.", remStr, firstIncorrect);
//                    state = 9;
//                }
//                else
//                {
//                    remStr += c.Char;
//                    HandleError("\nОжидалось число.", remStr, firstIncorrect);
//                }
//            }
//        }

//        private void State6()
//        {
//            Character c = stroke.GetNext();
//            if (c.Char >= '0' && c.Char <= '9')
//            {
//                state = 7;
//            }
//            else if (c.Char == '.')
//            {
//                state = 8;
//            }
//            else if (c.Char == ',')
//            {
//                state = 10;
//            }
//            else
//            {
//                String remStr = "";
//                Character firstIncorrect = c;

//                while (!IsDigit(stroke.Next().Char) && stroke.Next().Char != '.' && stroke.Next().Char != ',')
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                remStr += c.Char;
//                HandleError("\nНеожиданный символ: '" + firstIncorrect.Str + "'.", remStr, firstIncorrect);
//            }
//        }

//        private void State7()
//        {
//            Character c = stroke.GetNext();
//            if (c.Char >= '0' && c.Char <= '9')
//            {
//                state = 7;
//            }
//            else if (c.Char == '.')
//            {
//                state = 8;
//            }
//            else if (c.Char == ',')
//            {
//                state = 10;
//            }
//            else
//            {
//                String remStr = "";
//                Character firstIncorrect = c;

//                while (!IsDigit(stroke.Next().Char) && stroke.Next().Char != '.' && stroke.Next().Char != ',')
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                remStr += c.Char;
//                HandleError("\nНеожиданный символ: '" + firstIncorrect.Str + "'.", remStr, firstIncorrect);
//            }
//        }

//        private void State8()
//        {
//            Character c = stroke.GetNext();
//            if (c.Char >= '0' && c.Char <= '9')
//            {
//                state = 9;
//            }
//            else
//            {
//                String remStr = "";
//                Character firstIncorrect = c;

//                while (!IsDigit(stroke.Next().Char))
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                remStr += c.Char;
//                HandleError("\nНеожиданный символ: '" + firstIncorrect.Str + "'.", remStr, firstIncorrect);
//            }
//        }

//        private void State9()
//        {
//            Character c = stroke.GetNext();
//            if (c.Char >= '0' && c.Char <= '9')
//            {
//                state = 9;
//            }
//            else if (c.Char == ',')
//            {
//                state = 10;
//            }
//            else
//            {
//                String remStr = "";
//                Character firstIncorrect = c;

//                while (!IsDigit(stroke.Next().Char) && stroke.Next().Char != ',')
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                remStr += c.Char;
//                HandleError("\nНеожиданный символ: '" + firstIncorrect.Str + "'.", remStr, firstIncorrect);
//            }
//        }
//        private void State10()
//        {
//            Character c = stroke.GetNext();

//            if (c.Char == '+' || c.Char == '-')
//            {
//                state = 11;
//                stroke.GetNext();
//            }
//            else if (IsDigit(c.Char)) state = 11;
//            else if (c.Char == ')')
//            {
//                String remStr = "";
//                Character firstIncorrect = c;
//                HandleError("\nОжидалось число.", null, c);
//                state = 15;
//            }
//            else
//            {
//                String remStr = "";
//                Character firstIncorrect = c;

//                while (!IsDigit(stroke.Next().Char) && stroke.Next().Char != '+' && stroke.Next().Char != '-' && stroke.Next().Char != ')')
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                if (stroke.Next().Char == ')')
//                {
//                    HandleError("\nОжидалось число.", remStr, firstIncorrect);
//                    state = 14;
//                }
//                else
//                {
//                    remStr += c.Char;
//                    HandleError("\nОжидалось число.", remStr, firstIncorrect);
//                }
//            }
//        }
//        private void State11()
//        {
//            Character c = stroke.GetNext();
//            if (c.Char >= '0' && c.Char <= '9')
//            {
//                state = 12;
//            }
//            else if (c.Char == '.')
//            {
//                state = 13;
//            }
//            else if (c.Char == ')')
//            {
//                state = 15;
//            }
//            else
//            {
//                String remStr = "";
//                Character firstIncorrect = c;

//                while (!IsDigit(stroke.Next().Char) && stroke.Next().Char != '.' && stroke.Next().Char != ')')
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                remStr += c.Char;
//                HandleError("\nНеожиданный символ: '" + firstIncorrect.Str + "'.", remStr, firstIncorrect);
//            }
//        }
//        private void State12()
//        {
//            Character c = stroke.GetNext();
//            if (c.Char >= '0' && c.Char <= '9')
//            {
//                state = 12;
//            }
//            else if (c.Char == '.')
//            {
//                state = 13;
//            }
//            else if (c.Char == ')')
//            {
//                state = 15;
//            }
//            else
//            {
//                String remStr = "";
//                Character firstIncorrect = c;

//                while (!IsDigit(stroke.Next().Char) && stroke.Next().Char != '.' && stroke.Next().Char != ')')
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                remStr += c.Char;
//                HandleError("\nНеожиданный символ: '" + firstIncorrect.Str + "'.", remStr, firstIncorrect);
//            }
//        }
//        private void State13()
//        {
//            Character c = stroke.GetNext();
//            if (c.Char >= '0' && c.Char <= '9')
//            {
//                state = 14;
//            }
//            else
//            {
//                String remStr = "";
//                Character firstIncorrect = c;

//                while (!IsDigit(stroke.Next().Char))
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                remStr += c.Char;
//                HandleError("\nНеожиданный символ: '" + firstIncorrect.Str + "'.", remStr, firstIncorrect);
//            }
//        }

//        private void State14()
//        {
//            Character c = stroke.GetNext();
//            if (c.Char >= '0' && c.Char <= '9')
//            {
//                state = 14;
//            }
//            else if (c.Char == ')')
//            {
//                state = 15;
//            }
//            else
//            {
//                String remStr = "";
//                Character firstIncorrect = c;

//                while (!IsDigit(stroke.Next().Char) && stroke.Next().Char != ')')
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                remStr += c.Char;
//                HandleError("\nОжидался символ ')'.", remStr, firstIncorrect);
//                state = 15;
//                stroke.MoveBack();
//            }
//        }

//        private void State15()
//        {
//            Character c = stroke.GetNext();
//            if (c.Char == ';')
//            {
//                state = 16;
//            }
//            else
//            {
//                String remStr = "";
//                Character firstIncorrect = c;

//                while (stroke.Next().Char != ';')
//                {
//                    if (tryStop()) break;
//                    remStr += c.Char;
//                    c = stroke.GetNext();
//                }
//                remStr += c.Char;
//                HandleError("\nНеожиданный символ: '" + firstIncorrect.Str + "'.", remStr, firstIncorrect);
//            }
//        }
//        private bool IsLetter(char c)
//        {
//            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
//        }

//        private bool IsDigit(char c)
//        {
//            return (c >= '0' && c <= '9');
//        }
//    }
//}
