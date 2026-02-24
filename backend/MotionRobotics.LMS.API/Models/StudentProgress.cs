namespace MotionRobotics.LMS.API.Models
{
    /// <summary>
    /// Tracks student progress through experiments with teacher approval workflow.
    /// Sequential unlock: Student can only access next experiment after teacher approves current one.
    /// </summary>
    public class StudentProgress
    {
        public int Id { get; set; }

        // Student reference (now using StudentId instead of just email)
        public int StudentId { get; set; }
        public string StudentEmail { get; set; } = string.Empty;  // Kept for backward compatibility

        // Experiment reference
        public int ExperimentId { get; set; }

        // Completion tracking
        public bool Completed { get; set; } = false;
        public DateTime? CompletedAt { get; set; }
        public string? SubmissionNotes { get; set; }  // Student's notes when submitting
        public string? SubmissionImageUrl { get; set; }  // Photo proof of completed experiment

        // Teacher approval workflow
        public bool IsApprovedByTeacher { get; set; } = false;
        public int? ApprovedByTeacherId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? TeacherRemarks { get; set; }

        // Academic year tracking
        public int AcademicYearId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Student? Student { get; set; }
        public Experiment? Experiment { get; set; }
        public Teacher? ApprovedByTeacher { get; set; }
        public AcademicYear? AcademicYear { get; set; }
    }
}
