namespace MotionRobotics.LMS.API.Models
{
    /// <summary>
    /// Tracks which experiments are unlocked/assigned for each class.
    /// Teacher unlocks experiments one by one as they teach them.
    /// Only unlocked experiments are visible to students.
    /// </summary>
    public class ClassExperimentUnlock
    {
        public int Id { get; set; }

        public int ClassId { get; set; }
        public Class? Class { get; set; }

        public int ExperimentId { get; set; }
        public Experiment? Experiment { get; set; }

        /// <summary>
        /// Teacher who unlocked this experiment
        /// </summary>
        public int UnlockedByTeacherId { get; set; }
        public Teacher? UnlockedByTeacher { get; set; }

        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional deadline for students to complete this experiment
        /// </summary>
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// Teacher's notes/instructions for this experiment assignment
        /// </summary>
        public string? Instructions { get; set; }
    }
}
