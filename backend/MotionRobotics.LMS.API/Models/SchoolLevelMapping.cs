namespace MotionRobotics.LMS.API.Models
{
    /// <summary>
    /// Maps which standard/class gets which robotics level for each school per academic year.
    /// Example: School A -> Std 3 -> Level 1 (Mech Tech) for 2025-2026
    /// </summary>
    public class SchoolLevelMapping
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }
        public int ClassId { get; set; }  // Standard/Class (e.g., Std 3)
        public int RoboticsLevelId { get; set; }  // Which level this class gets
        public int AcademicYearId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public School? School { get; set; }
        public Class? Class { get; set; }
        public RoboticsLevel? RoboticsLevel { get; set; }
        public AcademicYear? AcademicYear { get; set; }
    }
}
