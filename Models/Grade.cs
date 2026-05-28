namespace StudentManagementSystem.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public double Marks { get; set; }
        public double MaxMarks { get; set; } = 100;
        public string ExamType { get; set; } = "Final";
        public DateTime ExamDate { get; set; } = DateTime.Today;
        public string Remarks { get; set; } = "";

        // Navigation
        public string StudentName { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string CourseCode { get; set; } = "";

        public double Percentage => MaxMarks > 0 ? (Marks / MaxMarks) * 100 : 0;

        public string LetterGrade => Percentage switch
        {
            >= 90 => "A+",
            >= 80 => "A",
            >= 70 => "B",
            >= 60 => "C",
            >= 50 => "D",
            _ => "F"
        };
    }
}
