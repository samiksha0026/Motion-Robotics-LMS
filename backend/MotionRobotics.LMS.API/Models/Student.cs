namespace MotionRobotics.LMS.API.Models
{
    /// <summary>
    /// Represents a student in the LMS system.
    /// Student's robotics level is determined by their school's level mapping (School + Class + Academic Year).
    /// </summary>
    public class Student
    {
        public int Id { get; set; }

        // Login credentials - links to AspNetUsers
        public string? UserId { get; set; }  // Reference to AspNetUsers
        public string Email { get; set; } = string.Empty;

        // Student details
        public string FullName { get; set; } = string.Empty;
        public string RollNo { get; set; } = string.Empty;
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }

        // School and class assignment
        public int SchoolId { get; set; }
        public int ClassId { get; set; }

        // Current academic year enrollment
        public int? CurrentAcademicYearId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public School? School { get; set; }
        public Class? Class { get; set; }
        public AcademicYear? CurrentAcademicYear { get; set; }
        public ICollection<StudentProgress> StudentProgresses { get; set; } = new List<StudentProgress>();
    }
}
