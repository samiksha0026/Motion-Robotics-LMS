namespace MotionRobotics.LMS.API.Models
{
    /// <summary>
    /// Tracks student's exam attempt and result.
    /// Pass/Fail determines if certificate is generated.
    /// </summary>
    public class ExamResult
    {
        public int Id { get; set; }

        // Student reference
        public int StudentId { get; set; }

        // Exam reference
        public int ExamId { get; set; }

        // Academic Year
        public int AcademicYearId { get; set; }

        // Result details
        public decimal ScoreObtained { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal Percentage { get; set; }
        public bool IsPassed { get; set; }

        // Answers stored as JSON
        public string? AnswersJson { get; set; }

        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
        public int TimeTakenSeconds { get; set; }  // How long student took

        // Certificate generated after passing
        public int? CertificateId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Student? Student { get; set; }
        public Exam? Exam { get; set; }
        public AcademicYear? AcademicYear { get; set; }
        public Certificate? Certificate { get; set; }
    }
}
