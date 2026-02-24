namespace MotionRobotics.LMS.API.DTOs.Teacher
{
    /// <summary>
    /// Teacher dashboard data
    /// </summary>
    public class TeacherDashboardDto
    {
        public int TeacherId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
        public int PendingApprovals { get; set; }
        public string CurrentAcademicYear { get; set; } = string.Empty;
        public List<AssignedClassDto> AssignedClasses { get; set; } = new();
        public List<PendingApprovalDto> RecentPendingApprovals { get; set; } = new();
    }

    /// <summary>
    /// Class assigned to teacher
    /// </summary>
    public class AssignedClassDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int RoboticsLevelId { get; set; }
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int ExperimentsCompleted { get; set; }
        public int TotalExperiments { get; set; }
        public int PendingApprovals { get; set; }
    }

    /// <summary>
    /// Student in a class
    /// </summary>
    public class ClassStudentDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int CompletedExperiments { get; set; }
        public int ApprovedExperiments { get; set; }
        public int TotalExperiments { get; set; }
        public int PendingApproval { get; set; }
        public decimal ProgressPercentage { get; set; }
    }

    /// <summary>
    /// Detailed class view with students
    /// </summary>
    public class ClassDetailDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int RoboticsLevelId { get; set; }
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelDescription { get; set; } = string.Empty;
        public int TotalExperiments { get; set; }
        public List<ClassStudentDto> Students { get; set; } = new();
        public List<ExperimentProgressDto> Experiments { get; set; } = new();
    }

    /// <summary>
    /// Experiment with class progress
    /// </summary>
    public class ExperimentProgressDto
    {
        public int ExperimentId { get; set; }
        public int SequenceOrder { get; set; }
        public string Title { get; set; } = string.Empty;
        public int EstimatedMinutes { get; set; }
        public int CompletedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
        public int TotalStudents { get; set; }
    }
}
