using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab1_compile
{
    internal class EditorTab : TabPage
    {
        public Stack<string> UndoStack = new Stack<string>();
        public Stack<string> RedoStack = new Stack<string>();

        public string FilePath { get; set; }
    }
}
