namespace MotionRobotics.LMS.API.DTOs.Student
{
    /// <summary>
    /// Student dashboard data
    /// </summary>
    public class StudentDashboardDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RollNo { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CurrentAcademicYear { get; set; } = string.Empty;
        public CurrentLevelDto? CurrentLevel { get; set; }
        public ProgressSummaryDto Progress { get; set; } = new();
        public ExperimentPreviewDto? NextExperiment { get; set; }
        public List<CertificateSummaryDto> Certificates { get; set; } = new();
    }

    /// <summary>
    /// Current robotics level info
    /// </summary>
    public class CurrentLevelDto
    {
        public int LevelId { get; set; }
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalExperiments { get; set; }
        public string? SyllabusUrl { get; set; }
    }

    /// <summary>
    /// Progress summary for student
    /// </summary>
    public class ProgressSummaryDto
    {
        public int TotalExperiments { get; set; }
        public int CompletedExperiments { get; set; }
        public int ApprovedExperiments { get; set; }
        public int PendingApproval { get; set; }
        public decimal ProgressPercentage { get; set; }
        public bool LevelCompleted { get; set; }
    }

    /// <summary>
    /// Short preview of an experiment
    /// </summary>
    public class ExperimentPreviewDto
    {
        public int ExperimentId { get; set; }
        public int SequenceOrder { get; set; }
        public string Title { get; set; } = string.Empty;
        public int EstimatedMinutes { get; set; }
    }

    /// <summary>
    /// Certificate summary for dashboard
    /// </summary>
    public class CertificateSummaryDto
    {
        public int CertificateId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
    }
}
