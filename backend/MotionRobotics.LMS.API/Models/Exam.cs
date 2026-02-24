namespace MotionRobotics.LMS.API.Models
{
    /// <summary>
    /// Represents a final exam for a robotics level.
    /// Exam unlocks only after all experiments in the level are completed and approved.
    /// </summary>
    public class Exam
    {
        public int Id { get; set; }

        // Link to RoboticsLevel
        public int RoboticsLevelId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; } = 30;
        public int TotalQuestions { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal PassingPercentage { get; set; } = 40;  // Minimum % to pass

        // Questions stored as JSON or separate table
        public string? QuestionsJson { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public RoboticsLevel? RoboticsLevel { get; set; }
        public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    }
}
