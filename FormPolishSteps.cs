using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Lab1_compile
{
    public partial class FormPolishSteps : Form
    {
        private DataGridView stepsGrid;
        public FormPolishSteps(List<PolishStep> steps)
        {
            InitializeComponent();
            this.Text = "Визуализация вычисления ПОЛИЗ";
            this.Size = new Size(700, 400);
            stepsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Consolas", 12)
            };
            stepsGrid.Columns.Add("Token", "Токен");
            stepsGrid.Columns.Add("StackState", "Состояние стека");
            stepsGrid.Columns.Add("Action", "Действие");
            foreach (var step in steps)
            {
                stepsGrid.Rows.Add(step.Token, step.StackState, step.Action);
            }
            this.Controls.Add(stepsGrid);
        }
    }
}