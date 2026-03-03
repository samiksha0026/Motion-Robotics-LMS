namespace MotionRobotics.LMS.API.DTOs.Admin
{
    /// <summary>
    /// DTO for certificate list response
    /// </summary>
    public class CertificateListDto
    {
        public List<CertificateDetailDto> Certificates { get; set; } = new();
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// DTO for certificate details
    /// </summary>
    public class CertificateDetailDto
    {
        public int Id { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string StudentRollNo { get; set; } = string.Empty;
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string? SchoolLogoUrl { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int RoboticsLevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public decimal ExamScore { get; set; }
        public decimal PassingScore { get; set; }
        public decimal Percentage { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CertificateFileUrl { get; set; }
        public DateTime IssuedDate { get; set; }
    }

    /// <summary>
    /// DTO for certificate verification (public)
    /// </summary>
    public class CertificateVerificationDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public CertificatePublicDto? Certificate { get; set; }
    }

    /// <summary>
    /// DTO for public certificate display
    /// </summary>
    public class CertificatePublicDto
    {
        public string CertificateNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for regenerating certificate
    /// </summary>
    public class RegenerateCertificateDto
    {
        public int CertificateId { get; set; }
        public string? CustomTitle { get; set; }
    }

    /// <summary>
    /// DTO for student progress overview (admin view)
    /// </summary>
    public class StudentProgressOverviewDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentRollNo { get; set; } = string.Empty;
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string RoboticsLevelName { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public int TotalExperiments { get; set; }
        public int CompletedExperiments { get; set; }
        public int ApprovedExperiments { get; set; }
        public int PendingApproval { get; set; }
        public decimal CompletionPercentage { get; set; }
        public bool IsEligibleForExam { get; set; }
        public bool HasPassedExam { get; set; }
        public bool HasCertificate { get; set; }
        public List<ExperimentProgressDetailDto> Experiments { get; set; } = new();
    }

    /// <summary>
    /// DTO for experiment progress detail
    /// </summary>
    public class ExperimentProgressDetailDto
    {
        public int ExperimentId { get; set; }
        public string ExperimentTitle { get; set; } = string.Empty;
        public int SequenceOrder { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByTeacher { get; set; }
        public string? TeacherRemarks { get; set; }
        public string Status { get; set; } = string.Empty; // NotStarted, InProgress, PendingApproval, Approved
    }

    /// <summary>
    /// DTO for progress statistics
    /// </summary>
    public class ProgressStatisticsDto
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int StudentsWithProgress { get; set; }
        public int StudentsCompletedLevel { get; set; }
        public int CertificatesIssued { get; set; }
        public decimal AverageCompletionRate { get; set; }
        public List<LevelProgressStatsDto> LevelStats { get; set; } = new();
    }

    /// <summary>
    /// DTO for level-wise progress statistics
    /// </summary>
    public class LevelProgressStatsDto
    {
        public int RoboticsLevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public int StudentsEnrolled { get; set; }
        public int StudentsCompleted { get; set; }
        public int CertificatesIssued { get; set; }
        public decimal CompletionRate { get; set; }
    }
}
