namespace MotionRobotics.LMS.API.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        // Login credentials - links to AspNetUsers
        public string? UserId { get; set; }  // Reference to AspNetUsers
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int SchoolId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public School? School { get; set; }
        public ICollection<TeacherClass> TeacherClasses { get; set; } = new List<TeacherClass>();
    }
}
