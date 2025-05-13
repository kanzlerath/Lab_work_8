using System;

namespace Lab1_compile
{
    class Character
    {
        private char _c;
        private int _idx;
        public char Char
        {
            get {return _c;}
        }
        public String Str
        {
            get
            {
                if (_c == '\0') return "<конец файла>";
                else if (_c == '\n') return "<новая строка>";
                else return "" + _c;
            }
        }

        public int Idx
        {
            get {return _idx;}
        }

        public Character(char c, int idx)
        {
            _c = c;
            _idx = idx;
        }
    }
    internal class Stroke
    {
        private char[] chars;
        private int index;

        public Stroke(string text)
        {
            chars = text.ToCharArray();
            index = 0;
        }

        public Character GetNext()
        {
            SkipSpaces();
            if (index == chars.Length) return new Character('\0', index);
            Character result = new Character(chars[index], index);
            index++;
            return result;
        }

        private bool isSpace(char c)
        {
            return (c == ' ' || c == '\t' || c == '\r');
        }

        public void SkipSpaces()
        {
            while (index < chars.Length && isSpace(chars[index])) index++;
        }

        public Character Next()
        {
            SkipSpaces();
            if (index == chars.Length) return new Character('\0', index);
            return new Character(chars[index], index);
        }

        public void MoveBack()
        {
            if (index > 0) index--;
        }
    }
}
