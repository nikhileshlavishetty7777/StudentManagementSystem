using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Database
{
    public static class DatabaseManager
    {
        private static readonly string DbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "StudentMS.db");

        private static string ConnectionString => $"Data Source={DbPath}";

        public static void Initialize()
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            string[] tables = {
                @"CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Email TEXT UNIQUE,
                    Phone TEXT,
                    DateOfBirth TEXT,
                    Address TEXT,
                    Gender TEXT,
                    EnrollmentDate TEXT,
                    Status TEXT DEFAULT 'Active'
                );",
                @"CREATE TABLE IF NOT EXISTS Courses (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CourseCode TEXT UNIQUE NOT NULL,
                    CourseName TEXT NOT NULL,
                    Description TEXT,
                    Credits INTEGER DEFAULT 3,
                    Instructor TEXT,
                    Department TEXT,
                    MaxStudents INTEGER DEFAULT 30
                );",
                @"CREATE TABLE IF NOT EXISTS Grades (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL,
                    CourseId INTEGER NOT NULL,
                    Marks REAL NOT NULL,
                    MaxMarks REAL DEFAULT 100,
                    ExamType TEXT DEFAULT 'Final',
                    ExamDate TEXT,
                    Remarks TEXT,
                    FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE,
                    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE
                );"
            };

            foreach (var sql in tables)
            {
                using var cmd = new SqliteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }

            SeedData(conn);
        }

        private static void SeedData(SqliteConnection conn)
        {
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Students", conn);
            long count = (long)(checkCmd.ExecuteScalar() ?? 0L);
            if (count > 0) return;

            string[] seedStudents = {
                "INSERT INTO Students (FirstName,LastName,Email,Phone,DateOfBirth,Gender,EnrollmentDate,Status) VALUES ('Aarav','Shah','aarav.shah@email.com','9876543210','2002-05-15','Male','2021-07-01','Active')",
                "INSERT INTO Students (FirstName,LastName,Email,Phone,DateOfBirth,Gender,EnrollmentDate,Status) VALUES ('Priya','Patel','priya.patel@email.com','9876543211','2003-03-20','Female','2022-07-01','Active')",
                "INSERT INTO Students (FirstName,LastName,Email,Phone,DateOfBirth,Gender,EnrollmentDate,Status) VALUES ('Rohan','Mehta','rohan.mehta@email.com','9876543212','2001-11-10','Male','2020-07-01','Active')",
            };
            string[] seedCourses = {
                "INSERT INTO Courses (CourseCode,CourseName,Credits,Instructor,Department) VALUES ('CS101','Introduction to Programming',4,'Dr. Sharma','Computer Science')",
                "INSERT INTO Courses (CourseCode,CourseName,Credits,Instructor,Department) VALUES ('MATH201','Calculus II',3,'Prof. Joshi','Mathematics')",
                "INSERT INTO Courses (CourseCode,CourseName,Credits,Instructor,Department) VALUES ('ENG101','Technical English',2,'Ms. Desai','English')",
            };

            foreach (var s in seedStudents)
            {
                using var cmd = new SqliteCommand(s, conn);
                cmd.ExecuteNonQuery();
            }
            foreach (var c in seedCourses)
            {
                using var cmd = new SqliteCommand(c, conn);
                cmd.ExecuteNonQuery();
            }

            string[] seedGrades = {
                "INSERT INTO Grades (StudentId,CourseId,Marks,MaxMarks,ExamType,ExamDate) VALUES (1,1,88,100,'Final','2024-05-10')",
                "INSERT INTO Grades (StudentId,CourseId,Marks,MaxMarks,ExamType,ExamDate) VALUES (1,2,75,100,'Final','2024-05-12')",
                "INSERT INTO Grades (StudentId,CourseId,Marks,MaxMarks,ExamType,ExamDate) VALUES (2,1,92,100,'Final','2024-05-10')",
                "INSERT INTO Grades (StudentId,CourseId,Marks,MaxMarks,ExamType,ExamDate) VALUES (3,3,65,100,'Final','2024-05-14')",
            };
            foreach (var g in seedGrades)
            {
                using var cmd = new SqliteCommand(g, conn);
                cmd.ExecuteNonQuery();
            }
        }

        public static List<Student> GetAllStudents(string search = "")
        {
            var list = new List<Student>();
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            string sql = "SELECT * FROM Students";
            if (!string.IsNullOrWhiteSpace(search))
                sql += " WHERE FirstName LIKE @s OR LastName LIKE @s OR Email LIKE @s OR Phone LIKE @s";
            sql += " ORDER BY FirstName";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@s", $"%{search}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapStudent(reader));
            return list;
        }

        public static Student? GetStudentById(int id)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqliteCommand("SELECT * FROM Students WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapStudent(reader) : null;
        }

        public static void AddStudent(Student s)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqliteCommand(@"INSERT INTO Students (FirstName,LastName,Email,Phone,DateOfBirth,Address,Gender,EnrollmentDate,Status) VALUES (@fn,@ln,@em,@ph,@dob,@addr,@gen,@enr,@st)", conn);
            BindStudent(cmd, s);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateStudent(Student s)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqliteCommand(@"UPDATE Students SET FirstName=@fn,LastName=@ln,Email=@em,Phone=@ph,DateOfBirth=@dob,Address=@addr,Gender=@gen,EnrollmentDate=@enr,Status=@st WHERE Id=@id", conn);
            BindStudent(cmd, s);
            cmd.Parameters.AddWithValue("@id", s.Id);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteStudent(int id)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqliteCommand("DELETE FROM Students WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private static void BindStudent(SqliteCommand cmd, Student s)
        {
            cmd.Parameters.AddWithValue("@fn",   s.FirstName);
            cmd.Parameters.AddWithValue("@ln",   s.LastName);
            cmd.Parameters.AddWithValue("@em",   s.Email);
            cmd.Parameters.AddWithValue("@ph",   s.Phone);
            cmd.Parameters.AddWithValue("@dob",  s.DateOfBirth.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@addr", s.Address);
            cmd.Parameters.AddWithValue("@gen",  s.Gender);
            cmd.Parameters.AddWithValue("@enr",  s.EnrollmentDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@st",   s.Status);
        }

        private static Student MapStudent(SqliteDataReader r) => new()
        {
            Id             = r.GetInt32(0),
            FirstName      = r.GetString(1),
            LastName       = r.GetString(2),
            Email          = r.IsDBNull(3) ? "" : r.GetString(3),
            Phone          = r.IsDBNull(4) ? "" : r.GetString(4),
            DateOfBirth    = r.IsDBNull(5) ? DateTime.Today : DateTime.Parse(r.GetString(5)),
            Address        = r.IsDBNull(6) ? "" : r.GetString(6),
            Gender         = r.IsDBNull(7) ? "" : r.GetString(7),
            EnrollmentDate = r.IsDBNull(8) ? DateTime.Today : DateTime.Parse(r.GetString(8)),
            Status         = r.IsDBNull(9) ? "Active" : r.GetString(9),
        };

        public static List<Course> GetAllCourses(string search = "")
        {
            var list = new List<Course>();
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            string sql = "SELECT * FROM Courses";
            if (!string.IsNullOrWhiteSpace(search))
                sql += " WHERE CourseName LIKE @s OR CourseCode LIKE @s OR Department LIKE @s";
            sql += " ORDER BY CourseCode";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@s", $"%{search}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapCourse(reader));
            return list;
        }

        public static void AddCourse(Course c)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqliteCommand(@"INSERT INTO Courses (CourseCode,CourseName,Description,Credits,Instructor,Department,MaxStudents) VALUES (@cc,@cn,@desc,@cr,@inst,@dept,@max)", conn);
            BindCourse(cmd, c);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateCourse(Course c)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqliteCommand(@"UPDATE Courses SET CourseCode=@cc,CourseName=@cn,Description=@desc,Credits=@cr,Instructor=@inst,Department=@dept,MaxStudents=@max WHERE Id=@id", conn);
            BindCourse(cmd, c);
            cmd.Parameters.AddWithValue("@id", c.Id);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteCourse(int id)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqliteCommand("DELETE FROM Courses WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private static void BindCourse(SqliteCommand cmd, Course c)
        {
            cmd.Parameters.AddWithValue("@cc",   c.CourseCode);
            cmd.Parameters.AddWithValue("@cn",   c.CourseName);
            cmd.Parameters.AddWithValue("@desc", c.Description);
            cmd.Parameters.AddWithValue("@cr",   c.Credits);
            cmd.Parameters.AddWithValue("@inst", c.Instructor);
            cmd.Parameters.AddWithValue("@dept", c.Department);
            cmd.Parameters.AddWithValue("@max",  c.MaxStudents);
        }

        private static Course MapCourse(SqliteDataReader r) => new()
        {
            Id          = r.GetInt32(0),
            CourseCode  = r.GetString(1),
            CourseName  = r.GetString(2),
            Description = r.IsDBNull(3) ? "" : r.GetString(3),
            Credits     = r.IsDBNull(4) ? 3  : r.GetInt32(4),
            Instructor  = r.IsDBNull(5) ? "" : r.GetString(5),
            Department  = r.IsDBNull(6) ? "" : r.GetString(6),
            MaxStudents = r.IsDBNull(7) ? 30 : r.GetInt32(7),
        };

        public static List<Grade> GetAllGrades(int? studentId = null, int? courseId = null)
        {
            var list = new List<Grade>();
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            string sql = @"SELECT g.*, s.FirstName||' '||s.LastName AS StudentName, c.CourseName, c.CourseCode FROM Grades g JOIN Students s ON g.StudentId = s.Id JOIN Courses c ON g.CourseId = c.Id WHERE 1=1";
            if (studentId.HasValue) sql += " AND g.StudentId = @sid";
            if (courseId.HasValue)  sql += " AND g.CourseId = @cid";
            sql += " ORDER BY g.ExamDate DESC";
            using var cmd = new SqliteCommand(sql, conn);
            if (studentId.HasValue) cmd.Parameters.AddWithValue("@sid", studentId.Value);
            if (courseId.HasValue)  cmd.Parameters.AddWithValue("@cid", courseId.Value);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapGrade(reader));
            return list;
        }

        public static void AddGrade(Grade g)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqliteCommand(@"INSERT INTO Grades (StudentId,CourseId,Marks,MaxMarks,ExamType,ExamDate,Remarks) VALUES (@sid,@cid,@marks,@max,@type,@date,@rem)", conn);
            BindGrade(cmd, g);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateGrade(Grade g)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqliteCommand(@"UPDATE Grades SET StudentId=@sid,CourseId=@cid,Marks=@marks,MaxMarks=@max,ExamType=@type,ExamDate=@date,Remarks=@rem WHERE Id=@id", conn);
            BindGrade(cmd, g);
            cmd.Parameters.AddWithValue("@id", g.Id);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteGrade(int id)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqliteCommand("DELETE FROM Grades WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private static void BindGrade(SqliteCommand cmd, Grade g)
        {
            cmd.Parameters.AddWithValue("@sid",   g.StudentId);
            cmd.Parameters.AddWithValue("@cid",   g.CourseId);
            cmd.Parameters.AddWithValue("@marks", g.Marks);
            cmd.Parameters.AddWithValue("@max",   g.MaxMarks);
            cmd.Parameters.AddWithValue("@type",  g.ExamType);
            cmd.Parameters.AddWithValue("@date",  g.ExamDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@rem",   g.Remarks);
        }

        private static Grade MapGrade(SqliteDataReader r) => new()
        {
            Id          = r.GetInt32(0),
            StudentId   = r.GetInt32(1),
            CourseId    = r.GetInt32(2),
            Marks       = r.GetDouble(3),
            MaxMarks    = r.IsDBNull(4)  ? 100     : r.GetDouble(4),
            ExamType    = r.IsDBNull(5)  ? "Final" : r.GetString(5),
            ExamDate    = r.IsDBNull(6)  ? DateTime.Today : DateTime.Parse(r.GetString(6)),
            Remarks     = r.IsDBNull(7)  ? ""      : r.GetString(7),
            StudentName = r.IsDBNull(8)  ? ""      : r.GetString(8),
            CourseName  = r.IsDBNull(9)  ? ""      : r.GetString(9),
            CourseCode  = r.IsDBNull(10) ? ""      : r.GetString(10),
        };

        public static (int students, int courses, int grades) GetDashboardStats()
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            long s = (long)(new SqliteCommand("SELECT COUNT(*) FROM Students WHERE Status='Active'", conn).ExecuteScalar() ?? 0L);
            long c = (long)(new SqliteCommand("SELECT COUNT(*) FROM Courses", conn).ExecuteScalar() ?? 0L);
            long g = (long)(new SqliteCommand("SELECT COUNT(*) FROM Grades", conn).ExecuteScalar() ?? 0L);
            return ((int)s, (int)c, (int)g);
        }

        public static double GetAverageMarks()
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var val = new SqliteCommand("SELECT AVG(Marks*100.0/MaxMarks) FROM Grades", conn).ExecuteScalar();
            return val == DBNull.Value || val == null ? 0 : Convert.ToDouble(val);
        }
    }
}