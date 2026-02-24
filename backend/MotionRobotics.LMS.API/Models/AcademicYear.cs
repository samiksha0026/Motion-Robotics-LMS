namespace MotionRobotics.LMS.API.Models
{
    /// <summary>
    /// Represents an academic year for tracking progress and assignments.
    /// Example: "2025-2026"
    /// </summary>
    public class AcademicYear
    {
        public int Id { get; set; }
        public string YearName { get; set; } = string.Empty;  // e.g., "2025-2026"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrent { get; set; } = false;  // Only one can be current
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<SchoolLevelMapping> SchoolLevelMappings { get; set; } = new List<SchoolLevelMapping>();
        public ICollection<StudentProgress> StudentProgresses { get; set; } = new List<StudentProgress>();
    }
}
