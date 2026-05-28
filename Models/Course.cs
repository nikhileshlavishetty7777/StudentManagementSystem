namespace StudentManagementSystem.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string Description { get; set; } = "";
        public int Credits { get; set; }
        public string Instructor { get; set; } = "";
        public string Department { get; set; } = "";
        public int MaxStudents { get; set; } = 30;
    }
}
