using System;
using System.Drawing;
using System.Windows.Forms;
using StudentManagementSystem.Database;
using StudentManagementSystem.Helpers;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Forms
{
    public partial class GradeForm : Form
    {
        private DataGridView dgv = null!;
        private TextBox searchBox = null!;
        private readonly bool _openAdd;

        public GradeForm(bool openAdd = false)
        {
            _openAdd = openAdd;
            InitializeComponent();
            BuildUI();
            LoadData();
            if (_openAdd) OpenAddDialog();
        }

        private void BuildUI()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = UIHelper.BgColor;

            var titleBar = new Panel { Height = 56, Dock = DockStyle.Top, BackColor = UIHelper.BgColor };
            var title = UIHelper.CreateLabel("Grades", UIHelper.FontTitle, UIHelper.TextPrimary);
            title.AutoSize = false; title.Dock = DockStyle.Left; title.Width = 300;
            title.TextAlign = ContentAlignment.MiddleLeft;
            var btnAdd = UIHelper.CreateButton("+ Add Grade", UIHelper.WarningColor); btnAdd.Width = 140; btnAdd.Dock = DockStyle.Right;
            btnAdd.Click += (s, e) => OpenAddDialog();
            titleBar.Controls.Add(btnAdd); titleBar.Controls.Add(title);

            var searchPanel = UIHelper.CreateSearchBar(out searchBox);
            searchBox.PlaceholderText = "🔍  Search by student or course...";
            searchBox.TextChanged += (s, e) => LoadData();

            dgv = UIHelper.CreateGrid();
            dgv.CellDoubleClick += (s, e) => EditSelected();

            var btnPanel = new Panel { Height = 48, Dock = DockStyle.Bottom, BackColor = UIHelper.BgColor, Padding = new Padding(0, 6, 0, 6) };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill };
            var btnEdit   = UIHelper.PrimaryBtn("✏ Edit");   btnEdit.Width = 110; btnEdit.Click += (s, e) => EditSelected();
            var btnDelete = UIHelper.DangerBtn("🗑 Delete"); btnDelete.Width = 110; btnDelete.Click += (s, e) => DeleteSelected();
            var btnRefresh = UIHelper.NeutralBtn("↻ Refresh"); btnRefresh.Width = 110; btnRefresh.Click += (s, e) => LoadData();
            flow.Controls.AddRange(new Control[] { btnEdit, btnDelete, btnRefresh });
            btnPanel.Controls.Add(flow);

            var gridCard = UIHelper.CreateCard(0); gridCard.Dock = DockStyle.Fill; gridCard.Controls.Add(dgv);
            this.Controls.Add(gridCard); this.Controls.Add(btnPanel); this.Controls.Add(searchPanel); this.Controls.Add(titleBar);
        }

        private void LoadData()
        {
            var grades = DatabaseManager.GetAllGrades();
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Id"); dt.Columns.Add("Student"); dt.Columns.Add("Course");
            dt.Columns.Add("Type"); dt.Columns.Add("Marks"); dt.Columns.Add("%");
            dt.Columns.Add("Grade"); dt.Columns.Add("Date"); dt.Columns.Add("Remarks");

            string filter = searchBox?.Text?.ToLower() ?? "";
            foreach (var g in grades)
            {
                if (!string.IsNullOrEmpty(filter) &&
                    !g.StudentName.ToLower().Contains(filter) &&
                    !g.CourseName.ToLower().Contains(filter) &&
                    !g.CourseCode.ToLower().Contains(filter)) continue;

                dt.Rows.Add(g.Id, g.StudentName, $"[{g.CourseCode}] {g.CourseName}", g.ExamType,
                    $"{g.Marks}/{g.MaxMarks}", $"{g.Percentage:F1}%", g.LetterGrade,
                    g.ExamDate.ToString("dd MMM yyyy"), g.Remarks);
            }
            dgv.DataSource = dt;
            if (dgv.Columns.Count > 0) dgv.Columns["Id"].Visible = false;

            // Color grade cells
            foreach (DataGridViewRow row in dgv.Rows)
            {
                var cell = row.Cells["Grade"];
                cell.Style.ForeColor = cell.Value?.ToString() switch
                {
                    "A+" or "A" => UIHelper.SuccessColor,
                    "B"         => UIHelper.AccentColor,
                    "C"         => UIHelper.WarningColor,
                    "D"         => Color.Orange,
                    _           => UIHelper.DangerColor,
                };
                cell.Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }
        }

        private void OpenAddDialog(Grade? grade = null)
        {
            var dlg = new GradeDialog(grade);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (grade == null) DatabaseManager.AddGrade(dlg.Grade);
                else DatabaseManager.UpdateGrade(dlg.Grade);
                LoadData();
            }
        }

        private void EditSelected()
        {
            if (dgv.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);
            var all = DatabaseManager.GetAllGrades();
            var grade = all.Find(g => g.Id == id);
            if (grade != null) OpenAddDialog(grade);
        }

        private void DeleteSelected()
        {
            if (dgv.SelectedRows.Count == 0) return;
            if (MessageBox.Show("Delete this grade record?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);
                DatabaseManager.DeleteGrade(id);
                LoadData();
            }
        }
    }

    public class GradeDialog : Form
    {
        public Grade Grade { get; private set; }
        private ComboBox cboStudent = null!, cboCourse = null!, cboExamType = null!;
        private NumericUpDown nudMarks = null!, nudMax = null!;
        private DateTimePicker dtpDate = null!;
        private TextBox txtRemarks = null!;

        public GradeDialog(Grade? grade = null)
        {
            Grade = grade ?? new Grade();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = Grade.Id == 0 ? "Add Grade" : "Edit Grade";
            this.Size = new Size(460, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UIHelper.BgColor;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var students = DatabaseManager.GetAllStudents();
            var courses  = DatabaseManager.GetAllCourses();

            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(24) };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            // Student
            AddLbl(panel, "Student *", 0);
            cboStudent = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontBody, Dock = DockStyle.Fill };
            foreach (var s in students) cboStudent.Items.Add(new ComboItem(s.Id, s.FullName));
            if (Grade.StudentId > 0) { foreach (ComboItem item in cboStudent.Items) if (item.Id == Grade.StudentId) { cboStudent.SelectedItem = item; break; } }
            panel.Controls.Add(cboStudent, 1, 0);

            // Course
            AddLbl(panel, "Course *", 1);
            cboCourse = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontBody, Dock = DockStyle.Fill };
            foreach (var c in courses) cboCourse.Items.Add(new ComboItem(c.Id, $"[{c.CourseCode}] {c.CourseName}"));
            if (Grade.CourseId > 0) { foreach (ComboItem item in cboCourse.Items) if (item.Id == Grade.CourseId) { cboCourse.SelectedItem = item; break; } }
            panel.Controls.Add(cboCourse, 1, 1);

            // ExamType
            AddLbl(panel, "Exam Type", 2);
            cboExamType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontBody, Dock = DockStyle.Fill };
            cboExamType.Items.AddRange(new[] { "Final", "Midterm", "Quiz", "Assignment", "Practical" });
            cboExamType.Text = Grade.ExamType.Length > 0 ? Grade.ExamType : "Final";
            panel.Controls.Add(cboExamType, 1, 2);

            // Marks
            AddLbl(panel, "Marks Obtained", 3);
            nudMarks = new NumericUpDown { Minimum = 0, Maximum = 10000, DecimalPlaces = 1, Value = (decimal)Grade.Marks, Font = UIHelper.FontBody, Dock = DockStyle.Fill };
            panel.Controls.Add(nudMarks, 1, 3);

            // Max Marks
            AddLbl(panel, "Max Marks", 4);
            nudMax = new NumericUpDown { Minimum = 1, Maximum = 10000, DecimalPlaces = 1, Value = (decimal)Math.Max(1, Grade.MaxMarks), Font = UIHelper.FontBody, Dock = DockStyle.Fill };
            panel.Controls.Add(nudMax, 1, 4);

            // Date
            AddLbl(panel, "Exam Date", 5);
            dtpDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = Grade.ExamDate == DateTime.MinValue ? DateTime.Today : Grade.ExamDate, Dock = DockStyle.Fill };
            panel.Controls.Add(dtpDate, 1, 5);

            // Remarks
            AddLbl(panel, "Remarks", 6);
            txtRemarks = new TextBox { Text = Grade.Remarks, Font = UIHelper.FontBody, Dock = DockStyle.Fill };
            panel.Controls.Add(txtRemarks, 1, 6);

            var btnRow = new Panel { Height = 52, Dock = DockStyle.Bottom, BackColor = UIHelper.BgColor, Padding = new Padding(24, 8, 24, 8) };
            var btnSave = UIHelper.CreateButton("💾 Save", UIHelper.WarningColor); btnSave.Width = 120; btnSave.Dock = DockStyle.Right; btnSave.Click += Save_Click;
            var btnCancel = UIHelper.NeutralBtn("Cancel"); btnCancel.Width = 100; btnCancel.Dock = DockStyle.Left; btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            btnRow.Controls.Add(btnSave); btnRow.Controls.Add(btnCancel);
            this.Controls.Add(panel); this.Controls.Add(btnRow);
        }

        private void AddLbl(TableLayoutPanel p, string text, int row)
        {
            var lbl = UIHelper.CreateLabel(text + ":"); lbl.Dock = DockStyle.Fill; lbl.TextAlign = ContentAlignment.MiddleLeft;
            p.Controls.Add(lbl, 0, row);
        }

        private void Save_Click(object? s, EventArgs e)
        {
            if (cboStudent.SelectedItem == null || cboCourse.SelectedItem == null)
            {
                MessageBox.Show("Please select a student and course.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Grade.StudentId = ((ComboItem)cboStudent.SelectedItem).Id;
            Grade.CourseId  = ((ComboItem)cboCourse.SelectedItem).Id;
            Grade.ExamType  = cboExamType.Text;
            Grade.Marks     = (double)nudMarks.Value;
            Grade.MaxMarks  = (double)nudMax.Value;
            Grade.ExamDate  = dtpDate.Value;
            Grade.Remarks   = txtRemarks.Text.Trim();
            this.DialogResult = DialogResult.OK;
        }
    }

    public class ComboItem
    {
        public int Id { get; }
        public string Name { get; }
        public ComboItem(int id, string name) { Id = id; Name = name; }
        public override string ToString() => Name;
    }
}
