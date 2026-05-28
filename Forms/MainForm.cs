using System;
using System.Drawing;
using System.Windows.Forms;
using StudentManagementSystem.Database;
using StudentManagementSystem.Helpers;

namespace StudentManagementSystem.Forms
{
    public partial class MainForm : Form
    {
        private Button? _activeNavBtn;

        public MainForm()
        {
            InitializeComponent();
            SetupSidebar();
            ShowDashboard();
        }

        private void SetupSidebar()
        {
            this.Text = "Student Management System";
            this.Size = new Size(1280, 760);
            this.MinimumSize = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UIHelper.BgColor;
            this.Font = UIHelper.FontBody;

            // ─── SIDEBAR ─────────────────────────────────────────────────────────
            var sidebar = new Panel
            {
                Width = 230,
                Dock = DockStyle.Left,
                BackColor = UIHelper.SidebarColor,
            };

            // Logo area
            var logoPanel = new Panel { Height = 80, Dock = DockStyle.Top, BackColor = UIHelper.SidebarColor };
            var logo = new Label
            {
                Text = "🎓  StudSys",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Dock = DockStyle.Fill,
            };
            logoPanel.Controls.Add(logo);
            sidebar.Controls.Add(logoPanel);

            // Nav buttons
            string[] navItems = { "📊  Dashboard", "👤  Students", "📚  Courses", "📝  Grades", "📈  Reports" };
            int top = 90;
            foreach (var item in navItems)
            {
                var btn = CreateNavButton(item, top);
                sidebar.Controls.Add(btn);
                top += 52;
            }

            // Footer
            var footer = new Label
            {
                Text = "v1.0  •  2024",
                Font = UIHelper.FontSmall,
                ForeColor = Color.FromArgb(100, 120, 150),
                Dock = DockStyle.Bottom,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            sidebar.Controls.Add(footer);

            // ─── CONTENT AREA ────────────────────────────────────────────────────
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIHelper.BgColor,
                Padding = new Padding(24),
            };

            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebar);
        }

        private Button CreateNavButton(string text, int top)
        {
            var btn = new Button
            {
                Text = text,
                Font = UIHelper.FontBody,
                ForeColor = Color.FromArgb(180, 195, 215),
                BackColor = UIHelper.SidebarColor,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand,
                Top = top,
                Left = 0,
                Width = 230,
                Height = 46,
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 55, 80);

            btn.Click += (s, e) => NavButton_Click(btn, text);
            return btn;
        }

        private void NavButton_Click(Button btn, string text)
        {
            // Reset previous
            if (_activeNavBtn != null)
            {
                _activeNavBtn.BackColor = UIHelper.SidebarColor;
                _activeNavBtn.ForeColor = Color.FromArgb(180, 195, 215);
            }
            btn.BackColor = UIHelper.PrimaryColor;
            btn.ForeColor = Color.White;
            _activeNavBtn = btn;

            contentPanel!.Controls.Clear();
            if (text.Contains("Dashboard")) ShowDashboard();
            else if (text.Contains("Student"))  { var f = new StudentForm(); f.TopLevel = false; f.Dock = DockStyle.Fill; contentPanel.Controls.Add(f); f.Show(); }
            else if (text.Contains("Course"))   { var f = new CourseForm();  f.TopLevel = false; f.Dock = DockStyle.Fill; contentPanel.Controls.Add(f); f.Show(); }
            else if (text.Contains("Grade"))    { var f = new GradeForm();   f.TopLevel = false; f.Dock = DockStyle.Fill; contentPanel.Controls.Add(f); f.Show(); }
            else if (text.Contains("Report"))   { var f = new ReportForm();  f.TopLevel = false; f.Dock = DockStyle.Fill; contentPanel.Controls.Add(f); f.Show(); }
        }

        private void ShowDashboard()
        {
            var (students, courses, grades) = DatabaseManager.GetDashboardStats();
            double avg = DatabaseManager.GetAverageMarks();

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = UIHelper.BgColor };

            // Title
            var title = new Label
            {
                Text = "Dashboard",
                Font = UIHelper.FontTitle,
                ForeColor = UIHelper.TextPrimary,
                AutoSize = false,
                Height = 50,
                Dock = DockStyle.Top,
            };

            // Stats row
            var statsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = UIHelper.BgColor,
                Padding = new Padding(0, 0, 0, 16),
            };

            statsFlow.Controls.Add(UIHelper.CreateStatCard("Active Students", students.ToString(), UIHelper.PrimaryColor));
            statsFlow.Controls.Add(UIHelper.CreateStatCard("Total Courses", courses.ToString(), UIHelper.AccentColor));
            statsFlow.Controls.Add(UIHelper.CreateStatCard("Total Grades", grades.ToString(), UIHelper.WarningColor));
            statsFlow.Controls.Add(UIHelper.CreateStatCard("Avg Score", $"{avg:F1}%", UIHelper.SuccessColor));

            // Quick actions
            var actionsTitle = new Label { Text = "Quick Actions", Font = UIHelper.FontSubtitle, ForeColor = UIHelper.TextPrimary, AutoSize = false, Height = 40, Dock = DockStyle.Top };
            var actionsFlow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, BackColor = UIHelper.BgColor, Padding = new Padding(0, 0, 0, 24) };

            var addStudentBtn = UIHelper.PrimaryBtn("+ Add Student"); addStudentBtn.Width = 140; addStudentBtn.Margin = new Padding(0, 0, 12, 0);
            var addCourseBtn  = UIHelper.SuccessBtn("+ Add Course");  addCourseBtn.Width  = 140; addCourseBtn.Margin  = new Padding(0, 0, 12, 0);
            var addGradeBtn   = UIHelper.CreateButton("+ Add Grade", UIHelper.WarningColor); addGradeBtn.Width = 140;

            addStudentBtn.Click += (s,e) => { new StudentForm(openAdd: true).ShowDialog(); };
            addCourseBtn.Click  += (s,e) => { new CourseForm(openAdd: true).ShowDialog(); };
            addGradeBtn.Click   += (s,e) => { new GradeForm(openAdd: true).ShowDialog(); };

            actionsFlow.Controls.AddRange(new Control[] { addStudentBtn, addCourseBtn, addGradeBtn });

            // Recent grades table
            var recentTitle = new Label { Text = "Recent Grade Entries", Font = UIHelper.FontSubtitle, ForeColor = UIHelper.TextPrimary, AutoSize = false, Height = 40, Dock = DockStyle.Top };
            var gridPanel = new Panel { Height = 280, Dock = DockStyle.Top, BackColor = Color.White };
            var grid = UIHelper.CreateGrid();
            var recentGrades = DatabaseManager.GetAllGrades();
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Student"); dt.Columns.Add("Course"); dt.Columns.Add("Type");
            dt.Columns.Add("Marks"); dt.Columns.Add("Grade"); dt.Columns.Add("Date");
            foreach (var g in recentGrades)
                dt.Rows.Add(g.StudentName, g.CourseName, g.ExamType, $"{g.Marks}/{g.MaxMarks}", g.LetterGrade, g.ExamDate.ToString("dd MMM yyyy"));
            grid.DataSource = dt;
            gridPanel.Controls.Add(grid);

            scroll.Controls.Add(gridPanel);
            scroll.Controls.Add(recentTitle);
            scroll.Controls.Add(actionsFlow);
            scroll.Controls.Add(actionsTitle);
            scroll.Controls.Add(statsFlow);
            scroll.Controls.Add(title);

            contentPanel!.Controls.Add(scroll);
        }
    }
}
