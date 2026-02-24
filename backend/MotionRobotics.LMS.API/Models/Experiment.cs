namespace MotionRobotics.LMS.API.Models
{
    /// <summary>
    /// Represents an experiment/activity within a robotics level.
    /// Experiments must be completed sequentially within each level.
    /// </summary>
    public class Experiment
    {
        public int Id { get; set; }

        // Link to RoboticsLevel instead of loose Level/ProgramName
        public int RoboticsLevelId { get; set; }

        // Sequence order within the level (1, 2, 3...) - determines unlock order
        public int SequenceOrder { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Objective { get; set; }
        public string? Components { get; set; }  // JSON or comma-separated list
        public string? Procedure { get; set; }  // Step-by-step instructions
        public string? WiringDiagram { get; set; }  // URL to diagram image
        public string? CircuitDiagram { get; set; }  // URL to circuit image
        public string? CodeSnippet { get; set; }
        public string? DemoVideoUrl { get; set; }
        public string? SafetyNotes { get; set; }
        public int EstimatedMinutes { get; set; } = 30;  // Expected completion time

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public RoboticsLevel? RoboticsLevel { get; set; }
        public ICollection<StudentProgress> StudentProgresses { get; set; } = new List<StudentProgress>();
    }
}
