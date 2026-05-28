using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StudentManagementSystem.Database;
using StudentManagementSystem.Helpers;

namespace StudentManagementSystem.Forms
{
    public partial class ReportForm : Form
    {
        public ReportForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = UIHelper.BgColor;

            var titleBar = new Panel { Height = 56, Dock = DockStyle.Top, BackColor = UIHelper.BgColor };
            var title = UIHelper.CreateLabel("Reports & Analytics", UIHelper.FontTitle, UIHelper.TextPrimary);
            title.AutoSize = false; title.Dock = DockStyle.Fill; title.TextAlign = ContentAlignment.MiddleLeft;
            titleBar.Controls.Add(title);

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = UIHelper.BgColor };

            // ── Grade Distribution ───────────────────────────────────────────────
            var secGrade = new Label { Text = "Grade Distribution", Font = UIHelper.FontSubtitle, ForeColor = UIHelper.TextPrimary, AutoSize = false, Height = 36, Dock = DockStyle.Top };

            var grades = DatabaseManager.GetAllGrades();
            var gradeGroups = grades.GroupBy(g => g.LetterGrade)
                .Select(x => (Grade: x.Key, Count: x.Count()))
                .OrderBy(x => x.Grade).ToList();

            var gradeFlow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, BackColor = UIHelper.BgColor };
            var gradeColors = new[] { UIHelper.SuccessColor, UIHelper.AccentColor, UIHelper.PrimaryColor, UIHelper.WarningColor, Color.Orange, UIHelper.DangerColor };
            string[] allGrades = { "A+", "A", "B", "C", "D", "F" };
            int ci = 0;
            foreach (var g in allGrades)
            {
                int cnt = gradeGroups.FirstOrDefault(x => x.Grade == g).Count;
                gradeFlow.Controls.Add(UIHelper.CreateStatCard(g, cnt.ToString(), gradeColors[ci++ % gradeColors.Length]));
            }

            // ── Student Status Distribution ───────────────────────────────────────
            var secStatus = new Label { Text = "Student Status", Font = UIHelper.FontSubtitle, ForeColor = UIHelper.TextPrimary, AutoSize = false, Height = 36, Dock = DockStyle.Top };
            var students = DatabaseManager.GetAllStudents();
            var statusFlow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, BackColor = UIHelper.BgColor };
            var statusGroups = students.GroupBy(s => s.Status).Select(x => (x.Key, x.Count())).ToList();
            Color[] sColors = { UIHelper.SuccessColor, UIHelper.TextSecondary, UIHelper.AccentColor, UIHelper.DangerColor };
            int si = 0;
            foreach (var (key, count) in statusGroups)
                statusFlow.Controls.Add(UIHelper.CreateStatCard(key, count.ToString(), sColors[si++ % sColors.Length]));

            // ── Top Students ─────────────────────────────────────────────────────
            var secTop = new Label { Text = "Top 10 Students by Average Score", Font = UIHelper.FontSubtitle, ForeColor = UIHelper.TextPrimary, AutoSize = false, Height = 36, Dock = DockStyle.Top };
            var topGrid = UIHelper.CreateGrid();
            topGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            var topDt = new System.Data.DataTable();
            topDt.Columns.Add("#"); topDt.Columns.Add("Student"); topDt.Columns.Add("Exams Taken"); topDt.Columns.Add("Avg Score (%)"); topDt.Columns.Add("Best Grade");

            var studentAvgs = grades
                .GroupBy(g => g.StudentName)
                .Select(g => new { Name = g.Key, Count = g.Count(), Avg = g.Average(x => x.Percentage), Best = g.OrderByDescending(x => x.Percentage).First().LetterGrade })
                .OrderByDescending(x => x.Avg)
                .Take(10).ToList();

            int rank = 1;
            foreach (var s in studentAvgs)
                topDt.Rows.Add(rank++, s.Name, s.Count, $"{s.Avg:F1}%", s.Best);

            topGrid.DataSource = topDt;
            var topPanel = new Panel { Height = 300, Dock = DockStyle.Top, BackColor = Color.White, Margin = new Padding(0, 0, 0, 24) };
            topPanel.Controls.Add(topGrid);

            // ── Course Enrollment ─────────────────────────────────────────────────
            var secCourse = new Label { Text = "Course Grade Summary", Font = UIHelper.FontSubtitle, ForeColor = UIHelper.TextPrimary, AutoSize = false, Height = 36, Dock = DockStyle.Top };
            var courses = DatabaseManager.GetAllCourses();
            var courseGrid = UIHelper.CreateGrid();
            var courseDt = new System.Data.DataTable();
            courseDt.Columns.Add("Course Code"); courseDt.Columns.Add("Course Name"); courseDt.Columns.Add("Entries"); courseDt.Columns.Add("Avg (%)"); courseDt.Columns.Add("Highest"); courseDt.Columns.Add("Lowest");

            foreach (var c in courses)
            {
                var cGrades = grades.Where(g => g.CourseName == c.CourseName).ToList();
                if (cGrades.Count == 0) { courseDt.Rows.Add(c.CourseCode, c.CourseName, 0, "N/A", "N/A", "N/A"); continue; }
                courseDt.Rows.Add(c.CourseCode, c.CourseName, cGrades.Count,
                    $"{cGrades.Average(g => g.Percentage):F1}%",
                    $"{cGrades.Max(g => g.Percentage):F1}%",
                    $"{cGrades.Min(g => g.Percentage):F1}%");
            }
            courseGrid.DataSource = courseDt;
            var coursePanel = new Panel { Height = 280, Dock = DockStyle.Top, BackColor = Color.White };
            coursePanel.Controls.Add(courseGrid);

            var btnRefresh = UIHelper.NeutralBtn("↻ Refresh Reports");
            btnRefresh.Width = 180; btnRefresh.Dock = DockStyle.Bottom;
            btnRefresh.Click += (s, e) => { this.Controls.Clear(); BuildUI(); };

            scroll.Controls.Add(coursePanel);
            scroll.Controls.Add(secCourse);
            scroll.Controls.Add(topPanel);
            scroll.Controls.Add(secTop);
            scroll.Controls.Add(statusFlow);
            scroll.Controls.Add(secStatus);
            scroll.Controls.Add(gradeFlow);
            scroll.Controls.Add(secGrade);

            this.Controls.Add(scroll);
            this.Controls.Add(titleBar);
        }
    }
}
