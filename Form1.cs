using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab1_compile
{

    public partial class Form1 : Form
    {
        private string currentFilePath = null;
        private Stack<string> undoStack = new Stack<string>();
        private Stack<string> redoStack = new Stack<string>();
        private StatusBarManager statusBar;
        private Font currentFont = new Font("Consolas", 14); // Шрифт по умолчанию
        private bool shouldClearHighlight = false;


        public Form1()
        {
            InitializeComponent();
            statusBar = new StatusBarManager(this);
            statusBar.HideCursorPosition(); // Скрываем метку при запуске
        }
        private void Editor_SelectionChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab)
            {
                if (sender is RichTextBox editor)
                {
                    statusBar.UpdateCursorPosition(editor);
                }
            }
            else
            {
                statusBar.HideCursorPosition();
            }
        }

        // Создать файл
        private void button1_Click(object sender, EventArgs e)
        {
            создатьToolStripMenuItem_Click(sender, e);
        }
        // Открыть файл
        private void button2_Click(object sender, EventArgs e)
        {
            открытьToolStripMenuItem_Click(sender, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            сохранитьToolStripMenuItem_Click(sender, e);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            отменитьToolStripMenuItem_Click(sender, e);
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorTab newTab = new EditorTab { Text = "Новый документ" };
            SplitContainer panel = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterWidth = 6,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Создаем панель для нумерации строк
            Panel panelLineNumbers = new Panel { Dock = DockStyle.Left, Width = 40, BackColor = Color.LightGray };
            RichTextBox editor = new RichTextBox { Dock = DockStyle.Fill, Visible = true };
            
            // Создаем DataGridView для вывода
            DataGridView outputGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Height = 200,
                Visible = true,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Увеличиваем высоту строки заголовков
            outputGrid.ColumnHeadersHeight = 40;
            outputGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Добавляем колонки
            outputGrid.Columns.Add("Type", "Тип");
            outputGrid.Columns.Add("Value", "Значение");
            outputGrid.Columns.Add("Position", "Позиция");
            outputGrid.Columns.Add("Details", "Детали");

            // Настраиваем ширину столбцов
            outputGrid.Columns["Type"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            outputGrid.Columns["Value"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            outputGrid.Columns["Position"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            outputGrid.Columns["Details"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Добавляем обработчик форматирования ячеек
            outputGrid.CellFormatting += (s, ev) =>
            {
                if (ev.RowIndex >= 0 && ev.ColumnIndex >= 0)
                {
                    DataGridViewRow row = outputGrid.Rows[ev.RowIndex];
                    if (row.Cells["Type"].Value?.ToString() == "Ошибка")
                    {
                        row.DefaultCellStyle.ForeColor = Color.Red;
                        row.DefaultCellStyle.Font = new Font(outputGrid.Font, FontStyle.Bold);
                    }
                }
            };

            panelLineNumbers.Tag = currentFont;

            editor.SelectionChanged += Editor_SelectionChanged;
            editor.TextChanged += (s, ev) => ApplySyntaxHighlighting(editor);

            editor.TextChanged += (s, ev) =>
            {
                undoTimer.Stop();
                undoTimer.Start();
                if (!newTab.Text.EndsWith("*")) newTab.Text += "*";
                panelLineNumbers.Invalidate();
            };
            editor.KeyPress += (s, ev) =>
            {
                if (shouldClearHighlight)
                {
                    ClearErrorHighlights(editor);
                    shouldClearHighlight = false;
                }
            };

            editor.VScroll += (s, ev) => panelLineNumbers.Invalidate();
            panelLineNumbers.Paint += (s, ev) => DrawLineNumbers(ev.Graphics, editor, panelLineNumbers);

            panel.Panel1.Controls.Add(editor);
            panel.Panel1.Controls.Add(panelLineNumbers);
            panel.Panel2.Controls.Add(outputGrid);
            newTab.Controls.Add(panel);

            tabControl1.TabPages.Add(newTab);
            tabControl1.SelectedTab = newTab;

            newTab.UndoStack.Push("");
            editor.Font = currentFont;
            outputGrid.Font = currentFont;

            tabControl1.SelectedTab.Text = "Новый документ";
        }

        private void DrawLineNumbers(Graphics g, RichTextBox editor, Panel panelLineNumbers)
        {
            g.Clear(Color.LightGray);

            Font font = panelLineNumbers.Tag as Font ?? editor.Font;

            int lineHeight = TextRenderer.MeasureText("0", font).Height;
            string[] lines = editor.Text.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                int y = i * lineHeight + 2;
                g.DrawString((i + 1).ToString(), font, Brushes.Black, panelLineNumbers.Width - 30, y);
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Текстовые файлы|*.txt|Все файлы|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;

                EditorTab newTab = new EditorTab { Text = Path.GetFileName(filePath), FilePath = filePath };
                SplitContainer panel = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    Orientation = Orientation.Horizontal,
                    SplitterWidth = 6,
                    BorderStyle = BorderStyle.FixedSingle
                };
                
                Panel panelLineNumbers = new Panel { Dock = DockStyle.Left, Width = 40, BackColor = Color.LightGray };
                RichTextBox editor = new RichTextBox { Dock = DockStyle.Fill, Visible = true };
                
                // Создаем DataGridView для вывода
                DataGridView outputGrid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    Height = 200,
                    Visible = true,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect
                };

                // Увеличиваем высоту строки заголовков
                outputGrid.ColumnHeadersHeight = 40;
                outputGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

                // Добавляем колонки
                outputGrid.Columns.Add("Type", "Тип");
                outputGrid.Columns.Add("Value", "Значение");
                outputGrid.Columns.Add("Position", "Позиция");
                outputGrid.Columns.Add("Details", "Детали");

                // Настраиваем ширину столбцов
                outputGrid.Columns["Type"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                outputGrid.Columns["Value"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                outputGrid.Columns["Position"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                outputGrid.Columns["Details"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                // Добавляем обработчик форматирования ячеек
                outputGrid.CellFormatting += (s, ev) =>
                {
                    if (ev.RowIndex >= 0 && ev.ColumnIndex >= 0)
                    {
                        DataGridViewRow row = outputGrid.Rows[ev.RowIndex];
                        if (row.Cells["Type"].Value?.ToString() == "Ошибка")
                        {
                            row.DefaultCellStyle.ForeColor = Color.Red;
                            row.DefaultCellStyle.Font = new Font(outputGrid.Font, FontStyle.Bold);
                        }
                    }
                };

                panelLineNumbers.Tag = currentFont;

                editor.SelectionChanged += Editor_SelectionChanged;
                editor.TextChanged += (s, ev) => ApplySyntaxHighlighting(editor);
                editor.TextChanged += (s, ev) =>
                {
                    undoTimer.Stop();
                    undoTimer.Start();
                    if (!newTab.Text.EndsWith("*")) newTab.Text += "*";
                    panelLineNumbers.Invalidate();
                };
                editor.VScroll += (s, ev) => panelLineNumbers.Invalidate();
                panelLineNumbers.Paint += (s, ev) => DrawLineNumbers(ev.Graphics, editor, panelLineNumbers);
                editor.Text = File.ReadAllText(filePath);
                editor.Tag = filePath;

                panel.Panel1.Controls.Add(editor);
                panel.Panel1.Controls.Add(panelLineNumbers);
                panel.Panel2.Controls.Add(outputGrid);
                newTab.Controls.Add(panel);

                tabControl1.TabPages.Add(newTab);
                tabControl1.SelectedTab = newTab;
                editor.Font = currentFont;
                outputGrid.Font = currentFont;

                newTab.UndoStack.Push(editor.Text);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.N:
                    создатьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.O:
                    открытьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.S:
                    сохранитьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.Shift | Keys.S:
                    сохранитьКакToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.W:
                    if (tabControl1.SelectedTab != null)
                        ЗакрытьВкладку(tabControl1.SelectedTab);
                    return true;

                case Keys.Control | Keys.Z:
                    отменитьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.Y:
                    повторитьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.X:
                    вырезатьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.C:
                    копироватьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.V:
                    вставитьToolStripMenuItem_Click(null, null);
                    return true;

                case Keys.Control | Keys.A:
                    выделитьВсеToolStripMenuItem_Click(null, null);
                    return true;

                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }
        /*private void ЗакрытьВкладку(TabPage tab)
        {
            if (tab is EditorTab editorTab)
            {
                Panel panel = editorTab.Controls[0] as Panel;
                RichTextBox editor = panel.Controls[0] as RichTextBox;

                // Проверяем, есть ли изменения
                if (editorTab.Text.EndsWith("*"))
                {
                    DialogResult result = MessageBox.Show(
                        $"Сохранить изменения в \"{editorTab.Text.TrimEnd('*')}\"?",
                        "Сохранение файла",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes) // Если выбрано "Сохранить"
                    {
                        if (string.IsNullOrEmpty(editorTab.FilePath)) // Новый файл, сохраняем как...
                        {
                            сохранитьКакToolStripMenuItem_Click(null, null);
                        }
                        else
                        {
                            File.WriteAllText(editorTab.FilePath, editor.Text);
                        }
                    }
                    else if (result == DialogResult.Cancel) // Если выбрано "Отмена"
                    {
                        return; // Не закрываем вкладку
                    }
                }

                tabControl1.TabPages.Remove(tab); // Удаляем вкладку
            }
        }*/

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                saveFileDialog1.Filter = "Текстовые файлы|*.txt|Все файлы|*.*";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog1.FileName, editor.Text);
                    tab.FilePath = saveFileDialog1.FileName;       // Сохраняем путь в свойство вкладки
                    editor.Tag = tab.FilePath;                      // (опционально) сохраняем путь в Tag редактора
                    tab.Text = Path.GetFileName(tab.FilePath);       // Обновляем название вкладки
                }
            }
            else
            {
                MessageBox.Show("Нет открытого документа", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (!string.IsNullOrEmpty(tab.FilePath)) // Если для вкладки уже задан путь
                {
                    File.WriteAllText(tab.FilePath, editor.Text);
                    tab.Text = Path.GetFileName(tab.FilePath);   // Обновляем название вкладки
                    editor.Tag = tab.FilePath;                     // Обновляем Tag, если он используется
                }
                else // Если файл новый, вызываем "Сохранить как"
                {
                    сохранитьКакToolStripMenuItem_Click(sender, e);
                }
            }
            else
            {
                MessageBox.Show("Нет открытого документа", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab selectedTab)
            {
                SplitContainer panel = selectedTab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                // Обновляем позицию курсора при смене вкладки
                statusBar.UpdateCursorPosition(editor);
            }
            else
            {
                // Если вкладок нет, скрываем метку позиции
                statusBar.HideCursorPosition();
            }
            if (tabControl1.SelectedTab != null)
            {
                SplitContainer panel = tabControl1.SelectedTab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (editor != null && !string.IsNullOrEmpty(editor.Text))
                {
                    currentFilePath = editor.Tag as string; // Используем Tag для хранения пути
                }
            }
        }
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {

            foreach (TabPage tabPage in tabControl1.TabPages)
            {
                if (tabPage is EditorTab tab)
                {
                    ЗакрытьВкладку(tab);
                    if (tabControl1.TabPages.Contains(tab)) // Если вкладка не закрылась (отмена)
                    {
                        return;
                    }
                }
            }

            // Если все вкладки обработаны — выходим из приложения
            Application.Exit();
        }
        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (tab.UndoStack.Count > 1) // Первый элемент — это начальное состояние
                {
                    tab.RedoStack.Push(tab.UndoStack.Pop());
                    editor.Text = tab.UndoStack.Peek();
                    editor.SelectionStart = editor.Text.Length;
                }
            }
        }
        private bool IsFileAlreadyOpen(string filePath)
        {
            foreach (TabPage tab in tabControl1.TabPages)
            {
                if (tab is EditorTab editorTab && editorTab.FilePath == filePath)
                {
                    tabControl1.SelectedTab = tab; // Просто активируем вкладку
                    return true;
                }
            }
            return false;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.AllowDrop = true;
            tabControl1.AllowDrop = true;  // Включаем поддержку на уровне вкладок

            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            tabControl1.DragEnter += new DragEventHandler(Form1_DragEnter);
            tabControl1.DragDrop += new DragEventHandler(Form1_DragDrop);
            foreach (TabPage tab in tabControl1.TabPages)
            {
                if (tab is EditorTab editorTab)
                {
                    SplitContainer panel = editorTab.Controls[0] as SplitContainer;
                    RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;
                    editor.TextChanged += (s, ev) => ApplySyntaxHighlighting(editor);
                }
            }

        }
        private void ApplySyntaxHighlighting(RichTextBox editor)
        {
            int selectionStart = editor.SelectionStart;
            int selectionLength = editor.SelectionLength;

            string[] keywords = { "int", "float", "double", "string", "if", "else", "while", "for", "return", "void" };
            string[] operators = { "+", "-", "*", "/", "=", "==", "!=", "<", ">", "<=", ">=" };

            editor.SuspendLayout();
            int cursorPosition = editor.SelectionStart;

            // Очищаем всю подсветку
            editor.SelectAll();
            editor.SelectionColor = Color.Black;

            // Подсвечиваем ключевые слова (синий)
            foreach (string keyword in keywords)
            {
                HighlightWord(editor, keyword, Color.Blue);
            }

            // Подсвечиваем числа (фиолетовый)
            HighlightRegex(editor, @"\b\d+\b", Color.Purple);

            // Подсвечиваем строки в кавычках (зеленый)
            HighlightRegex(editor, "\".*?\"", Color.Green);

            // Подсвечиваем операторы (красный)
            foreach (string op in operators)
            {
                HighlightWord(editor, op, Color.Red);
            }

            editor.SelectionStart = cursorPosition;
            editor.SelectionLength = 0;
            editor.SelectionColor = Color.Black;
            editor.ResumeLayout();

        }
        private void HighlightWord(RichTextBox editor, string word, Color color)
        {
            int index = 0;
            while ((index = editor.Text.IndexOf(word, index)) != -1)
            {
                editor.Select(index, word.Length);
                editor.SelectionColor = color;
                index += word.Length;
            }
        }
        private void HighlightRegex(RichTextBox editor, string pattern, Color color)
        {
            foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(editor.Text, pattern))
            {
                editor.Select(match.Index, match.Length);
                editor.SelectionColor = color;
            }
        }
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy; // Разрешаем копирование
            }
            else
            {
                e.Effect = DragDropEffects.None; // Отклоняем остальные типы
            }
        }
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string filePath in files) // Если тянут несколько файлов, открываем все
            {
                if (Path.GetExtension(filePath).ToLower() != ".txt") // Только txt-файлы
                {
                    MessageBox.Show($"Файл {Path.GetFileName(filePath)} не поддерживается!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }

                if (IsFileAlreadyOpen(filePath))
                {
                    continue; // Файл уже открыт, пропускаем его
                }

                EditorTab newTab = new EditorTab { Text = Path.GetFileName(filePath), FilePath = filePath };
                SplitContainer panel = new SplitContainer { Dock = DockStyle.Fill };
                Panel panelLineNumbers = new Panel { Dock = DockStyle.Left, Width = 40, BackColor = Color.LightGray };
                RichTextBox editor = new RichTextBox { Dock = DockStyle.Fill, Visible = true };
                RichTextBox output = new RichTextBox { Dock = DockStyle.Bottom, Height = 100, Visible = true, ReadOnly = true };
                panelLineNumbers.Tag = currentFont; // Сохраняем шрифт в Tag
                editor.SelectionChanged += Editor_SelectionChanged;
                editor.TextChanged += (s, ev) => ApplySyntaxHighlighting(editor);
                editor.TextChanged += (s, ev) =>
                {
                    undoTimer.Stop();
                    undoTimer.Start();
                    if (!newTab.Text.EndsWith("*")) newTab.Text += "*"; // Добавляем звездочку при изменении
                    panelLineNumbers.Invalidate(); // Перерисовка номеров строк
                };
                editor.VScroll += (s, ev) => panelLineNumbers.Invalidate();
                panelLineNumbers.Paint += (s, ev) => DrawLineNumbers(ev.Graphics, editor, panelLineNumbers);
                editor.Text = File.ReadAllText(filePath);
                editor.Tag = filePath;

                panel.Panel1.Controls.Add(editor);
                panel.Panel1.Controls.Add(panelLineNumbers); // Добавляем панель нумерации
                panel.Panel2.Controls.Add(output);
                newTab.Controls.Add(panel);

                tabControl1.TabPages.Add(newTab);
                tabControl1.SelectedTab = newTab;
                editor.Font = currentFont; // Применяем текущий шрифт
                output.Font = currentFont;

                newTab.UndoStack.Push(editor.Text); // Запоминаем изначальное состояние файла
            }
        }
        private void HighlightError(RichTextBox editor, int start, int end)
        {
            // Больше не выделяем ошибки в редакторе
        }
        private void ClearErrorHighlights(RichTextBox editor)
        {
            editor.SelectAll();
            editor.SelectionBackColor = editor.BackColor;
            editor.SelectionColor = editor.ForeColor;
            editor.SelectionFont = editor.Font; // Сбросить стиль на обычный
            editor.DeselectAll();
        }
        private void undoTimer_Tick(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (tab.UndoStack.Count == 0 || tab.UndoStack.Peek() != editor.Text)
                {
                    tab.UndoStack.Push(editor.Text); // Запоминаем состояние текста
                    tab.RedoStack.Clear();          // Очищаем redo, если пользователь что-то ввел
                }

                if (tab.RedoStack.Count > 0 && tab.RedoStack.Peek() == editor.Text)
                {
                    tab.RedoStack.Pop(); // Если повтор случайно закинул текущее состояние — убираем его
                }
            }
            undoTimer.Stop();
        }
        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (tab.RedoStack.Count > 0)
                {
                    string text = tab.RedoStack.Pop();  // Забираем текст из Redo
                    tab.UndoStack.Push(text);           // Пихаем обратно в Undo
                    editor.Text = text;                // Показываем в редакторе
                    editor.SelectionStart = editor.Text.Length;

                    undoTimer.Stop();
                    undoTimer.Start(); // Снова запускаем таймер
                }
            }
        }
        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (editor.SelectedText.Length > 0)
                {
                    Clipboard.SetText(editor.SelectedText);
                }
            }
        }
        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (Clipboard.ContainsText())
                {
                    editor.SelectedText = Clipboard.GetText(); // Вставляем в место выделения
                }
            }
        }
        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (editor.SelectedText.Length > 0)
                {
                    Clipboard.SetText(editor.SelectedText);
                    editor.SelectedText = ""; // Заменяем выделенный текст пустотой
                }
            }
        }
        private void выделитьВсеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                editor.SelectAll();
            }
        }
        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                if (editor.SelectedText.Length > 0)
                {
                    editor.SelectedText = ""; // Удаляет выделенный текст
                }
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            повторитьToolStripMenuItem_Click(sender, e);
        }
        private void button6_Click(object sender, EventArgs e)
        {
            копироватьToolStripMenuItem_Click(sender, e);
        }
        private void button7_Click(object sender, EventArgs e)
        {
            вырезатьToolStripMenuItem_Click(sender, e);
        }
        private void button8_Click(object sender, EventArgs e)
        {
            вставитьToolStripMenuItem_Click(sender, e);
        }
        private void button9_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab is EditorTab tab)
            {
                SplitContainer panel = tab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;
                DataGridView outputGrid = panel.Panel2.Controls[0] as DataGridView;
                ClearErrorHighlights(editor);
                
                // Очищаем таблицу
                outputGrid.Rows.Clear();
                outputGrid.Columns.Clear();
                outputGrid.Columns.Add("Type", "Тип");
                outputGrid.Columns.Add("Value", "Значение");
                outputGrid.Columns.Add("Position", "Позиция");
                outputGrid.Columns.Add("Description", "Описание");
                
                // Лексический анализ
                Lexer lexer = new Lexer(editor.Text);
                List<Token> tokens = lexer.Tokenize();
                List<ParseError> errors = lexer.GetErrors();
                
                // Если есть лексические ошибки — выводим только ошибки
                if (errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        outputGrid.Rows.Add(
                            "Ошибка",
                            error.Token.Value,
                            $"{error.Token.StartPosition}-{error.Token.EndPosition}",
                            error.Message
                        );
                        HighlightError(editor, error.Token.StartPosition, error.Token.EndPosition);
                    }
                    return;
                }
                
                // Синтаксический анализ
                RecursiveDescentParser parser = new RecursiveDescentParser(tokens);
                if (!parser.Parse())
                {
                    foreach (var error in parser.GetErrors())
                    {
                        outputGrid.Rows.Add(
                            "Ошибка",
                            error.Token.Value,
                            $"{error.Token.StartPosition}-{error.Token.EndPosition}",
                            error.Message
                        );
                        HighlightError(editor, error.Token.StartPosition, error.Token.EndPosition);
                    }
                    return;
                }
                
                // Если ошибок нет — выводим токены и ПОЛИЗ
                foreach (var token in tokens)
                {
                    outputGrid.Rows.Add(
                        token.Type,
                        token.Value,
                        $"{token.StartPosition}-{token.EndPosition}",
                        token.Description
                    );
                }
                PolishNotation polish = new PolishNotation();
                List<string> polishNotation = polish.ConvertToPolishNotation(tokens);
                outputGrid.Rows.Add(-2, "---", "---", "Польская инверсная запись (ПОЛИЗ)");
                string polizString = string.Join(" ", polishNotation);
                outputGrid.Rows.Add(-2, polizString, "---", "Результат преобразования");
                try
                {
                    double result = polish.EvaluatePolishNotation(polishNotation);
                    outputGrid.Rows.Add(-2, result.ToString(), "---", "Результат вычисления");
                    // Визуализация шагов
                    var steps = polish.EvaluatePolishNotationWithSteps(polishNotation);
                    var stepsForm = new FormPolishSteps(steps);
                    stepsForm.Show();
                }
                catch (Exception ex)
                {
                    outputGrid.Rows.Add("Ошибка", "-", "-", $"Ошибка вычисления: {ex.Message}");
                }
            }
        }
        private void ЗакрытьВкладку(TabPage tab)
        {
            if (tab is EditorTab editorTab)
            {
                SplitContainer panel = editorTab.Controls[0] as SplitContainer;
                RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;

                // Проверяем, есть ли изменения
                if (editorTab.Text.EndsWith("*"))
                {
                    DialogResult result = MessageBox.Show(
                        $"Сохранить изменения в \"{editorTab.Text.TrimEnd('*')}\"?",
                        "Сохранение файла",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes) // Если выбрано "Сохранить"
                    {
                        if (string.IsNullOrEmpty(editorTab.FilePath)) // Новый файл, сохраняем как...
                        {
                            сохранитьКакToolStripMenuItem_Click(null, null);
                        }
                        else
                        {
                            File.WriteAllText(editorTab.FilePath, editor.Text);
                        }
                    }
                    else if (result == DialogResult.Cancel) // Если выбрано "Отмена"
                    {
                        return; // Не закрываем вкладку
                    }
                }

                tabControl1.TabPages.Remove(tab); // Удаляем вкладку
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (TabPage tabPage in tabControl1.TabPages)
            {
                if (tabPage is EditorTab tab)
                {
                    ЗакрытьВкладку(tab);
                    if (tabControl1.TabPages.Contains(tab)) // Если вкладка не закрылась (отмена)
                    {
                        e.Cancel = true; // Прерываем закрытие программы
                        return;
                    }
                }
            }
        }
        private void шрифтToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FontDialog fontDialog = new FontDialog())
            {
                fontDialog.ShowColor = false;
                fontDialog.Font = currentFont; // Используем текущий шрифт

                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    currentFont = fontDialog.Font; // Сохраняем выбранный шрифт

                    foreach (TabPage tab in tabControl1.TabPages)
                    {
                        if (tab is EditorTab editorTab)
                        {
                            SplitContainer panel = editorTab.Controls[0] as SplitContainer;
                            RichTextBox editor = panel.Panel1.Controls[0] as RichTextBox;
                            RichTextBox output = panel.Panel2.Controls.Count >= 1 ? panel.Panel2.Controls[0] as RichTextBox : null;

                            editor.Font = currentFont;
                            if (output != null) output.Font = currentFont;
                            Panel panelLineNumbers = panel.Panel1.Controls
                            .OfType<Panel>()
                            .FirstOrDefault(p => p.Dock == DockStyle.Left);

                            if (panelLineNumbers != null)
                            {
                                panelLineNumbers.Tag = currentFont;
                                panelLineNumbers.Invalidate(); // Перерисовать номера строк
                            }
                        }
                    }
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
            "Название программы: Синтаксический анализатор комплексных чисел\nАвтор: Лужков Н.Д.\nГруппа: АП-226\nДисциплина: Теория формальных языков и компиляторов \n",
            "О программе",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
            );
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
            "Название программы: Синтаксический анализатор комплексных чисел\nАвтор: Лужков Н.Д.\nГруппа: АП-226\nДисциплина: Теория формальных языков и компиляторов \n",
            "О программе",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
            );
        }

        private void пускToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button9_Click(sender, e);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "About.html");

            try
            {
                // Сохраняем встроенный HTML в файл
                File.WriteAllText(tempPath, Properties.Resources.about1);
                // Открываем в браузере
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть справку: " + ex.Message);
            }
        }

        private void постановкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "Task.html");

            try
            {
                // Сохраняем встроенный HTML в файл
                File.WriteAllText(tempPath, Properties.Resources.task1);
                // Открываем в браузере
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть файл \"Постановка задачи\": " + ex.Message);
            }
        }

        private void грамматикаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "Grammar.html");

            try
            {
                // Сохраняем встроенный HTML в файл
                File.WriteAllText(tempPath, Properties.Resources.grammar);
                // Открываем в браузере
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть файл \"Грамматика\": " + ex.Message);
            }
        }

        private void классификацияГрамматикиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "Classification.html");

            try
            {
                // Сохраняем встроенный HTML в файл
                File.WriteAllText(tempPath, Properties.Resources.classification);
                // Открываем в браузере
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть файл \"Классификация грамматики\": " + ex.Message);
            }
        }

        private void методАнализаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string imagePath = Path.Combine(Path.GetTempPath(), "analysis_graph.png");
                string tempPath = Path.Combine(Path.GetTempPath(), "Analysis.html");
                Properties.Resources.Scheme1.Save(imagePath);
                string html = Properties.Resources.analysis1;
                html = html.Replace("src=\"image.png\"", $"src=\"file:///{imagePath.Replace("\\", "/")}\"");
                File.WriteAllText(tempPath, html);
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть файл \"Метод анализа\": " + ex.Message);
            }
        }

        private void диагностикаИНейтрализацияОшибокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string imagePath1 = Path.Combine(Path.GetTempPath(), "Tree1.png");
                string imagePath2 = Path.Combine(Path.GetTempPath(), "Tree2.png");
                string tempPath = Path.Combine(Path.GetTempPath(), "Errors.html");
                Properties.Resources.tree1.Save(imagePath1);
                Properties.Resources.tree2.Save(imagePath2);
                string html = Properties.Resources.errors;
                html = html.Replace("src=\"irons_tree_1.png\"", $"src=\"file:///{imagePath1.Replace("\\", "/")}\"");
                html = html.Replace("src=\"irons_tree_error.png\"", $"src=\"file:///{imagePath2.Replace("\\", "/")}\"");
                File.WriteAllText(tempPath, html);
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть файл \"Диагностика и нейтрализация ошибок\": " + ex.Message);
            }
        }

        private void вызовСправкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button10_Click(sender, e);
        }


        private void тестовыйПримерToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                string imagePath1 = Path.Combine(Path.GetTempPath(), "Test1.png");
                string imagePath2 = Path.Combine(Path.GetTempPath(), "Test2.png");
                string imagePath3 = Path.Combine(Path.GetTempPath(), "Test3.png");
                string imagePath4 = Path.Combine(Path.GetTempPath(), "Test4.png");
                string tempPath = Path.Combine(Path.GetTempPath(), "test_example.html");
                Properties.Resources.Test1.Save(imagePath1);
                Properties.Resources.Test2.Save(imagePath2);
                Properties.Resources.Test3.Save(imagePath3);
                Properties.Resources.Test4.Save(imagePath4);
                string html = Properties.Resources.test_example;
                html = html.Replace("src=\"Test1.png\"", $"src=\"file:///{imagePath1.Replace("\\", "/")}\"");
                html = html.Replace("src=\"Test2.png\"", $"src=\"file:///{imagePath2.Replace("\\", "/")}\"");
                html = html.Replace("src=\"Test3.png\"", $"src=\"file:///{imagePath3.Replace("\\", "/")}\"");
                html = html.Replace("src=\"Test4.png\"", $"src=\"file:///{imagePath4.Replace("\\", "/")}\"");
                File.WriteAllText(tempPath, html);
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть файл \"Тестовый пример\": " + ex.Message);
            }
        }

        private void списокЛитературыToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "Bibliography.html");

            try
            {
                // Сохраняем встроенный HTML в файл
                File.WriteAllText(tempPath, Properties.Resources.bibliography);
                // Открываем в браузере
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть файл \"Список литературы\": " + ex.Message);
            }
        }

        private void исходныйКодПрограммыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "Listing.html");

            try
            {
                // Сохраняем встроенный HTML в файл
                File.WriteAllText(tempPath, Properties.Resources.listing);
                // Открываем в браузере
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть файл \"Исходный код программы\": " + ex.Message);
            }
        }
    }
}