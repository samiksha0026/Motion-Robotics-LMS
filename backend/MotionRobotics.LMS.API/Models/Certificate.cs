namespace MotionRobotics.LMS.API.Models
{
    /// <summary>
    /// Represents a certificate issued to a student upon completing a robotics level.
    /// Certificate includes school logo and is issued per academic year.
    /// </summary>
    public class Certificate
    {
        public int Id { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;  // Unique certificate ID

        // Student reference
        public int StudentId { get; set; }
        public string StudentEmail { get; set; } = string.Empty;  // Kept for backward compatibility
        public string StudentName { get; set; } = string.Empty;

        // School reference (for logo on certificate)
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string? SchoolLogoUrl { get; set; }

        // Level completed
        public int RoboticsLevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;  // e.g., "Mech Tech"
        public int LevelNumber { get; set; }  // 1-6

        // Academic year
        public int AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;  // e.g., "2025-2026"

        // Exam score
        public decimal ExamScore { get; set; }
        public decimal PassingScore { get; set; }

        public string Title { get; set; } = "Certificate of Completion";
        public string? CertificateFileUrl { get; set; }  // Generated PDF URL
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Student? Student { get; set; }
        public School? School { get; set; }
        public RoboticsLevel? RoboticsLevel { get; set; }
        public AcademicYear? AcademicYear { get; set; }
    }
}
