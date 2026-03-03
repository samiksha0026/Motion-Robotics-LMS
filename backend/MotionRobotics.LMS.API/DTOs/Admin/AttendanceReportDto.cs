namespace MotionRobotics.LMS.API.DTOs.Admin
{
    // Comprehensive Reports
    public class ComprehensiveStudentReportDto
    {
        public int SchoolId { get; set; }
        public StudentInfoDto StudentInfo { get; set; } = new();
        public ProgressReportSectionDto ProgressReport { get; set; } = new();
        public ExamReportSectionDto ExamReport { get; set; } = new();
        public CertificateReportSectionDto CertificateReport { get; set; } = new();
    }

    public class StudentInfoDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CurrentLevel { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProgressReportSectionDto
    {
        public int TotalExperiments { get; set; }
        public int CompletedExperiments { get; set; }
        public int ApprovedExperiments { get; set; }
        public double CompletionPercentage { get; set; }
        public string ProgressStatus { get; set; } = string.Empty;
        public List<LevelProgressDto> LevelProgress { get; set; } = new();
    }

    public class LevelProgressDto
    {
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int TotalExperiments { get; set; }
        public int CompletedExperiments { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class ExamReportSectionDto
    {
        public int TotalExamsTaken { get; set; }
        public int ExamsPassed { get; set; }
        public int ExamsFailed { get; set; }
        public double AverageScore { get; set; }
        public double HighestScore { get; set; }
        public List<ExamResultSummaryDto> ExamResults { get; set; } = new();
    }

    public class ExamResultSummaryDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public double Score { get; set; }
        public double PassingScore { get; set; }
        public bool Passed { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    public class CertificateReportSectionDto
    {
        public int TotalCertificates { get; set; }
        public List<CertificateSummaryDto> Certificates { get; set; } = new();
    }

    public class CertificateSummaryDto
    {
        public int CertificateId { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
    }

    // School Comprehensive Report
    public class ComprehensiveSchoolReportDto
    {
        public SchoolInfoDto SchoolInfo { get; set; } = new();
        public SchoolStatisticsDto Statistics { get; set; } = new();
        public List<ClassSummaryReportDto> ClassReports { get; set; } = new();
        public List<TopPerformerDto> TopPerformers { get; set; } = new();
    }

    public class SchoolInfoDto
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class SchoolStatisticsDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalClasses { get; set; }
        public int TotalCertificatesIssued { get; set; }
        public double AverageExamScore { get; set; }
    }

    public class ClassSummaryReportDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public double AverageProgress { get; set; }
    }

    public class TopPerformerDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Category { get; set; } = string.Empty; // Attendance, Exam, Progress
    }

    // Monthly/Yearly Report
    public class PeriodReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; } = string.Empty; // "Monthly", "Weekly", "Yearly"
        public int TotalStudentsEnrolled { get; set; }
        public int NewEnrollments { get; set; }
        public int CertificatesIssued { get; set; }
        public int ExamsConducted { get; set; }
        public double AverageExamScore { get; set; }
        public List<LevelWiseReportDto> LevelWiseData { get; set; } = new();
    }

    public class LevelWiseReportDto
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int StudentsInLevel { get; set; }
        public int StudentsCompleted { get; set; }
        public double AverageScore { get; set; }
    }
}
