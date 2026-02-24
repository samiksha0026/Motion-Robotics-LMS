namespace MotionRobotics.LMS.API.Models
{
    /// <summary>
    /// Represents the 6 fixed robotics levels in the system.
    /// These are global and cannot be modified by schools.
    /// </summary>
    public class RoboticsLevel
    {
        public int Id { get; set; }
        public int LevelNumber { get; set; }  // 1-6
        public string Name { get; set; } = string.Empty;  // e.g., "Mech Tech"
        public string Description { get; set; } = string.Empty;
        public string? SyllabusUrl { get; set; }  // Path to syllabus PDF
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();
        public ICollection<SchoolLevelMapping> SchoolLevelMappings { get; set; } = new List<SchoolLevelMapping>();
    }
}
