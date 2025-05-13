using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1_compile
{
    public class ParseError
    {
        public string Message { get; }
        public Token Token { get; }

        public ParseError(string message, Token token)
        {
            Message = message;
            Token = token;
        }
    }
}
