using System;
using System.Drawing;
using System.Windows.Forms;
using StudentManagementSystem.Database;
using StudentManagementSystem.Helpers;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Forms
{
    public partial class StudentForm : Form
    {
        private DataGridView dgv = null!;
        private TextBox searchBox = null!;
        private readonly bool _openAdd;

        public StudentForm(bool openAdd = false)
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

            // Title bar
            var titleBar = new Panel { Height = 56, Dock = DockStyle.Top, BackColor = UIHelper.BgColor, Padding = new Padding(0, 8, 0, 8) };
            var title = UIHelper.CreateLabel("Students", UIHelper.FontTitle, UIHelper.TextPrimary);
            title.AutoSize = false;
            title.Dock = DockStyle.Left;
            title.Width = 300;
            title.TextAlign = ContentAlignment.MiddleLeft;

            var btnAdd = UIHelper.PrimaryBtn("+ Add Student"); btnAdd.Width = 140; btnAdd.Dock = DockStyle.Right;
            btnAdd.Click += (s, e) => OpenAddDialog();

            titleBar.Controls.Add(btnAdd);
            titleBar.Controls.Add(title);

            // Search
            var searchPanel = UIHelper.CreateSearchBar(out searchBox);
            searchBox.TextChanged += (s, e) => LoadData(searchBox.Text);

            // Grid
            dgv = UIHelper.CreateGrid();
            dgv.CellDoubleClick += (s, e) => EditSelected();

            // Buttons row
            var btnPanel = new Panel { Height = 48, Dock = DockStyle.Bottom, BackColor = UIHelper.BgColor, Padding = new Padding(0, 6, 0, 6) };
            var btnEdit   = UIHelper.PrimaryBtn("✏ Edit");   btnEdit.Width = 110;   btnEdit.Click   += (s, e) => EditSelected();
            var btnDelete = UIHelper.DangerBtn("🗑 Delete"); btnDelete.Width = 110; btnDelete.Click += (s, e) => DeleteSelected();
            var btnRefresh = UIHelper.NeutralBtn("↻ Refresh"); btnRefresh.Width = 110; btnRefresh.Click += (s, e) => LoadData();
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            flow.Controls.AddRange(new Control[] { btnEdit, btnDelete, btnRefresh });
            btnPanel.Controls.Add(flow);

            var gridCard = UIHelper.CreateCard(0);
            gridCard.Dock = DockStyle.Fill;
            gridCard.Controls.Add(dgv);

            this.Controls.Add(gridCard);
            this.Controls.Add(btnPanel);
            this.Controls.Add(searchPanel);
            this.Controls.Add(titleBar);
        }

        private void LoadData(string search = "")
{
    var students = DatabaseManager.GetAllStudents(search);

    var dt = new System.Data.DataTable();

    dt.Columns.Add("Id");
    dt.Columns.Add("Name");
    dt.Columns.Add("Email");
    dt.Columns.Add("Phone");
    dt.Columns.Add("Gender");
    dt.Columns.Add("Age");
    dt.Columns.Add("Enrolled");
    dt.Columns.Add("Status");

    foreach (var s in students)
    {
        dt.Rows.Add(
            s.Id,
            s.FullName,
            s.Email,
            s.Phone,
            s.Gender,
            s.Age,
            s.EnrollmentDate.ToString("dd MMM yyyy"),
            s.Status
        );
    }

    dgv.DataSource = null;   // 🔥 reset binding (IMPORTANT)
    dgv.DataSource = dt;

    if (dgv.Columns.Count > 0)
        dgv.Columns["Id"].Visible = false;
}

        private void OpenAddDialog(Student? student = null)
{
    var dlg = new StudentDialog(student);

    if (dlg.ShowDialog() == DialogResult.OK)
    {
        if (student == null)
        {
            DatabaseManager.AddStudent(dlg.Student);
        }
        else
        {
            DatabaseManager.UpdateStudent(dlg.Student);
        }

        // 🔥 IMPORTANT FIX
        LoadData("");        // reload data
        dgv.Refresh();       // force UI refresh
        dgv.ClearSelection();// optional but clean UI
    }
}

        private void EditSelected()
        {
            if (dgv.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);
            var student = DatabaseManager.GetStudentById(id);
            if (student != null) OpenAddDialog(student);
        }

        private void DeleteSelected()
        {
            if (dgv.SelectedRows.Count == 0) return;
            string name = dgv.SelectedRows[0].Cells["Name"].Value?.ToString() ?? "";
            if (MessageBox.Show($"Delete student '{name}'?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);
                DatabaseManager.DeleteStudent(id);
                LoadData();
            }
        }
    }

    // ─── ADD/EDIT DIALOG ─────────────────────────────────────────────────────────
    public class StudentDialog : Form
    {
        public Student Student { get; private set; }
        private TextBox txtFirst = null!, txtLast = null!, txtEmail = null!,
                        txtPhone = null!, txtAddress = null!;
        private ComboBox cboGender = null!, cboStatus = null!;
        private DateTimePicker dtpDob = null!, dtpEnroll = null!;

        public StudentDialog(Student? student = null)
        {
            Student = student ?? new Student();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = Student.Id == 0 ? "Add Student" : "Edit Student";
            this.Size = new Size(520, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UIHelper.BgColor;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 9, Padding = new Padding(24) };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            txtFirst   = AddField(panel, "First Name *", 0, 0);
            txtLast    = AddField(panel, "Last Name *",  0, 1);
            txtEmail   = AddField(panel, "Email",        1, 0, colSpan: 2);
            txtPhone   = AddField(panel, "Phone",        2, 0);
            cboGender  = AddCombo(panel, "Gender", new[] { "", "Male", "Female", "Other" }, 2, 1);
            AddLabel(panel, "Date of Birth", 3, 0);
            dtpDob = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = Student.DateOfBirth == DateTime.MinValue ? new DateTime(2000,1,1) : Student.DateOfBirth };
            panel.Controls.Add(dtpDob, 1, 3);
            AddLabel(panel, "Enrollment Date", 4, 0);
            dtpEnroll = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = Student.EnrollmentDate == DateTime.MinValue ? DateTime.Today : Student.EnrollmentDate };
            panel.Controls.Add(dtpEnroll, 1, 4);
            cboStatus  = AddCombo(panel, "Status", new[] { "Active", "Inactive", "Graduated", "Suspended" }, 5, 0);
            txtAddress = AddFieldMulti(panel, "Address", 6, 0);

            // Pre-fill
            txtFirst.Text  = Student.FirstName;
            txtLast.Text   = Student.LastName;
            txtEmail.Text  = Student.Email;
            txtPhone.Text  = Student.Phone;
            txtAddress.Text = Student.Address;
            cboGender.Text = Student.Gender;
            cboStatus.Text = Student.Status;

            // Buttons
            var btnRow = new Panel { Height = 52, Dock = DockStyle.Bottom, BackColor = UIHelper.BgColor, Padding = new Padding(24, 8, 24, 8) };
            var btnSave = UIHelper.PrimaryBtn("💾 Save"); btnSave.Width = 120; btnSave.Dock = DockStyle.Right;
            var btnCancel = UIHelper.NeutralBtn("Cancel");  btnCancel.Width = 100; btnCancel.Dock = DockStyle.Left;
            btnSave.Click += Save_Click;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            btnRow.Controls.Add(btnSave);
            btnRow.Controls.Add(btnCancel);

            this.Controls.Add(panel);
            this.Controls.Add(btnRow);
        }

        private TextBox AddField(TableLayoutPanel panel, string label, int row, int col, int colSpan = 1)
        {
            AddLabel(panel, label, row, col);
            var tb = UIHelper.CreateTextBox();
            tb.Dock = DockStyle.Fill;
            panel.Controls.Add(tb, col == 0 ? 0 : 1, row);
            if (colSpan > 1) panel.SetColumnSpan(tb, colSpan);
            return tb;
        }

        private TextBox AddFieldMulti(TableLayoutPanel panel, string label, int row, int col)
        {
            AddLabel(panel, label, row, col);
            var tb = new TextBox { Multiline = true, Height = 60, Font = UIHelper.FontBody, Dock = DockStyle.Fill };
            panel.Controls.Add(tb, 1, row);
            return tb;
        }

        private void AddLabel(TableLayoutPanel panel, string text, int row, int col)
        {
            var lbl = UIHelper.CreateLabel(text + ":");
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleLeft;
            panel.Controls.Add(lbl, col, row);
        }

        private ComboBox AddCombo(TableLayoutPanel panel, string label, string[] items, int row, int col)
        {
            AddLabel(panel, label, row, col);
            var cbo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = UIHelper.FontBody, Dock = DockStyle.Fill };
            cbo.Items.AddRange(items);
            panel.Controls.Add(cbo, col == 0 ? 0 : 1, row);
            return cbo;
        }

        private void Save_Click(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirst.Text) || string.IsNullOrWhiteSpace(txtLast.Text))
            {
                MessageBox.Show("First Name and Last Name are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Student.FirstName      = txtFirst.Text.Trim();
            Student.LastName       = txtLast.Text.Trim();
            Student.Email          = txtEmail.Text.Trim();
            Student.Phone          = txtPhone.Text.Trim();
            Student.Address        = txtAddress.Text.Trim();
            Student.Gender         = cboGender.Text;
            Student.Status         = cboStatus.Text.Length > 0 ? cboStatus.Text : "Active";
            Student.DateOfBirth    = dtpDob.Value;
            Student.EnrollmentDate = dtpEnroll.Value;
            this.DialogResult = DialogResult.OK;
        }
    }
}
