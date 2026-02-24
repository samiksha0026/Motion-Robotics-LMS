namespace MotionRobotics.LMS.API.Models
{
    public class TeacherClass
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int ClassId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Teacher? Teacher { get; set; }
        public Class? Class { get; set; }
    }
}
