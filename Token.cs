using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1_compile
{
    public class Token
    {
        public int Type { get; }
        public string Description { get; }
        public string Value { get; }
        public int StartPosition { get; }
        public int EndPosition { get; }

        public Token(int type, string description, string value, int startPosition, int endPosition)
        {
            Type = type;
            Description = description;
            Value = value;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }

        public override string ToString()
        {
            return $"{Description} '{Value}' (позиции {StartPosition}-{EndPosition})";
        }
    }
}