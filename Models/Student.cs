namespace StudentManagementSystem.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = "";
        public string Gender { get; set; } = "";
        public DateTime EnrollmentDate { get; set; } = DateTime.Today;
        public string Status { get; set; } = "Active";

        public string FullName => $"{FirstName} {LastName}";
        public int Age => DateTime.Today.Year - DateOfBirth.Year -
            (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    }
}
