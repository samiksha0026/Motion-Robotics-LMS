namespace MotionRobotics.LMS.API.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public int TeacherId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool IsPresent { get; set; }
        public string? Remarks { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Student? Student { get; set; }
        public Class? Class { get; set; }
        public Teacher? Teacher { get; set; }
    }
}
