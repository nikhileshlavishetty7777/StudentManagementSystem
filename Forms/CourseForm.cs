using System;
using System.Drawing;
using System.Windows.Forms;
using StudentManagementSystem.Database;
using StudentManagementSystem.Helpers;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Forms
{
    public partial class CourseForm : Form
    {
        private DataGridView dgv = null!;
        private TextBox searchBox = null!;
        private readonly bool _openAdd;

        public CourseForm(bool openAdd = false)
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
            var title = UIHelper.CreateLabel("Courses", UIHelper.FontTitle, UIHelper.TextPrimary);
            title.AutoSize = false; title.Dock = DockStyle.Left; title.Width = 300;
            title.TextAlign = ContentAlignment.MiddleLeft;
            var btnAdd = UIHelper.SuccessBtn("+ Add Course"); btnAdd.Width = 140; btnAdd.Dock = DockStyle.Right;
            btnAdd.Click += (s, e) => OpenAddDialog();
            titleBar.Controls.Add(btnAdd); titleBar.Controls.Add(title);

            var searchPanel = UIHelper.CreateSearchBar(out searchBox);
            searchBox.TextChanged += (s, e) => LoadData(searchBox.Text);

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

        private void LoadData(string search = "")
        {
            var courses = DatabaseManager.GetAllCourses(search);
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Id"); dt.Columns.Add("Code"); dt.Columns.Add("Course Name");
            dt.Columns.Add("Credits"); dt.Columns.Add("Instructor"); dt.Columns.Add("Department"); dt.Columns.Add("Max");
            foreach (var c in courses)
                dt.Rows.Add(c.Id, c.CourseCode, c.CourseName, c.Credits, c.Instructor, c.Department, c.MaxStudents);
            dgv.DataSource = dt;
            if (dgv.Columns.Count > 0) dgv.Columns["Id"].Visible = false;
        }

        private void OpenAddDialog(Course? course = null)
        {
            var dlg = new CourseDialog(course);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (course == null) DatabaseManager.AddCourse(dlg.Course);
                else DatabaseManager.UpdateCourse(dlg.Course);
                LoadData();
            }
        }

        private void EditSelected()
        {
            if (dgv.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);
            var all = DatabaseManager.GetAllCourses();
            var course = all.Find(c => c.Id == id);
            if (course != null) OpenAddDialog(course);
        }

        private void DeleteSelected()
        {
            if (dgv.SelectedRows.Count == 0) return;
            string name = dgv.SelectedRows[0].Cells["Course Name"].Value?.ToString() ?? "";
            if (MessageBox.Show($"Delete course '{name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);
                DatabaseManager.DeleteCourse(id);
                LoadData();
            }
        }
    }

    public class CourseDialog : Form
    {
        public Course Course { get; private set; }
        private TextBox txtCode = null!, txtName = null!, txtDesc = null!, txtInstructor = null!, txtDept = null!;
        private NumericUpDown nudCredits = null!, nudMax = null!;

        public CourseDialog(Course? course = null)
        {
            Course = course ?? new Course();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = Course.Id == 0 ? "Add Course" : "Edit Course";
            this.Size = new Size(500, 440);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UIHelper.BgColor;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(24) };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            txtCode       = AddField(panel, "Course Code *", 0);
            txtName       = AddField(panel, "Course Name *", 1);
            txtInstructor = AddField(panel, "Instructor",    2);
            txtDept       = AddField(panel, "Department",    3);
            AddLabelRow(panel, "Credits", 4);
            nudCredits = new NumericUpDown { Minimum = 1, Maximum = 10, Value = Math.Max(1, Course.Credits), Font = UIHelper.FontBody, Dock = DockStyle.Fill };
            panel.Controls.Add(nudCredits, 1, 4);
            AddLabelRow(panel, "Max Students", 5);
            nudMax = new NumericUpDown { Minimum = 1, Maximum = 500, Value = Math.Max(1, Course.MaxStudents), Font = UIHelper.FontBody, Dock = DockStyle.Fill };
            panel.Controls.Add(nudMax, 1, 5);
            txtDesc = AddFieldMulti(panel, "Description", 6);

            txtCode.Text       = Course.CourseCode;
            txtName.Text       = Course.CourseName;
            txtInstructor.Text = Course.Instructor;
            txtDept.Text       = Course.Department;
            txtDesc.Text       = Course.Description;

            var btnRow = new Panel { Height = 52, Dock = DockStyle.Bottom, BackColor = UIHelper.BgColor, Padding = new Padding(24, 8, 24, 8) };
            var btnSave   = UIHelper.SuccessBtn("💾 Save"); btnSave.Width = 120; btnSave.Dock = DockStyle.Right; btnSave.Click += Save_Click;
            var btnCancel = UIHelper.NeutralBtn("Cancel");  btnCancel.Width = 100; btnCancel.Dock = DockStyle.Left; btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            btnRow.Controls.Add(btnSave); btnRow.Controls.Add(btnCancel);

            this.Controls.Add(panel); this.Controls.Add(btnRow);
        }

        private TextBox AddField(TableLayoutPanel p, string label, int row)
        {
            AddLabelRow(p, label, row);
            var tb = UIHelper.CreateTextBox(); tb.Dock = DockStyle.Fill;
            p.Controls.Add(tb, 1, row); return tb;
        }

        private TextBox AddFieldMulti(TableLayoutPanel p, string label, int row)
        {
            AddLabelRow(p, label, row);
            var tb = new TextBox { Multiline = true, Height = 55, Font = UIHelper.FontBody, Dock = DockStyle.Fill };
            p.Controls.Add(tb, 1, row); return tb;
        }

        private void AddLabelRow(TableLayoutPanel p, string text, int row)
        {
            var lbl = UIHelper.CreateLabel(text + ":"); lbl.Dock = DockStyle.Fill; lbl.TextAlign = ContentAlignment.MiddleLeft;
            p.Controls.Add(lbl, 0, row);
        }

        private void Save_Click(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Course Code and Name are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Course.CourseCode = txtCode.Text.Trim().ToUpper();
            Course.CourseName = txtName.Text.Trim();
            Course.Instructor = txtInstructor.Text.Trim();
            Course.Department = txtDept.Text.Trim();
            Course.Description = txtDesc.Text.Trim();
            Course.Credits = (int)nudCredits.Value;
            Course.MaxStudents = (int)nudMax.Value;
            this.DialogResult = DialogResult.OK;
        }
    }
}
