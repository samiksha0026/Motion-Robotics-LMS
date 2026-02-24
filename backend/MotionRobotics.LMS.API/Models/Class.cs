namespace MotionRobotics.LMS.API.Models
{
    public class Class
    {
        public int Id { get; set; }
        public string ClassName { get; set; } = string.Empty;  // e.g., "Class 6", "Standard 8"
        public int SchoolId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public School? School { get; set; }
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
