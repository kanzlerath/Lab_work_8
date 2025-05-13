using System;
using System.Windows.Forms;

namespace Lab1_compile
{
    internal class StatusBarManager
    {
        private StatusStrip statusStrip;
        private ToolStripStatusLabel languageLabel;
        private ToolStripStatusLabel cursorPositionLabel;
        private Form1 form;

        public StatusBarManager(Form1 form)
        {
            this.form = form;

            statusStrip = new StatusStrip();

            languageLabel = new ToolStripStatusLabel { Text = "Язык: " + GetCurrentKeyboardLanguage() };
            cursorPositionLabel = new ToolStripStatusLabel { Text = "Позиция: 1" };

            statusStrip.Items.Add(languageLabel);
            statusStrip.Items.Add(cursorPositionLabel);

            form.Controls.Add(statusStrip);

            // Подписка на смену языка клавиатуры
            form.InputLanguageChanged += Form_InputLanguageChanged;
        }

        public void UpdateCursorPosition(RichTextBox editor)
        {
            int position = editor.SelectionStart;
            int lineStart = editor.GetFirstCharIndexOfCurrentLine();
            int column = position - lineStart + 1; // Считаем с 1

            cursorPositionLabel.Text = $"Позиция: {column}";
        }

        private void Form_InputLanguageChanged(object sender, InputLanguageChangedEventArgs e)
        {
            languageLabel.Text = "Язык: " + GetCurrentKeyboardLanguage();
        }

        private string GetCurrentKeyboardLanguage()
        {
            return InputLanguage.CurrentInputLanguage.Culture.TwoLetterISOLanguageName.ToUpper();
        }
        public void HideCursorPosition()
        {
            cursorPositionLabel.Text = "";
        }
    }
}
