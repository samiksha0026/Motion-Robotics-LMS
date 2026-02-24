namespace MotionRobotics.LMS.API.DTOs.Teacher
{
    /// <summary>
    /// DTO for teacher viewing exam results of their students
    /// </summary>
    public class TeacherExamResultDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentRollNo { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string RoboticsLevelName { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public decimal ScoreObtained { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal Percentage { get; set; }
        public bool IsPassed { get; set; }
        public int TimeTakenSeconds { get; set; }
        public string TimeTakenFormatted { get; set; } = string.Empty;
        public DateTime AttemptedAt { get; set; }
        public bool CertificateGenerated { get; set; }
    }

    /// <summary>
    /// DTO for exam results list (teacher view)
    /// </summary>
    public class TeacherExamResultsListDto
    {
        public List<TeacherExamResultDto> Results { get; set; } = new();
        public int TotalCount { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public decimal AveragePercentage { get; set; }
    }

    /// <summary>
    /// DTO for class exam summary
    /// </summary>
    public class ClassExamSummaryDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int StudentsAttempted { get; set; }
        public int StudentsPassed { get; set; }
        public int StudentsFailed { get; set; }
        public int StudentsPending { get; set; }
        public decimal PassRate { get; set; }
        public decimal AverageScore { get; set; }
    }

    /// <summary>
    /// DTO for student exam eligibility
    /// </summary>
    public class StudentExamEligibilityDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RollNo { get; set; } = string.Empty;
        public bool IsEligible { get; set; }
        public int TotalExperiments { get; set; }
        public int CompletedExperiments { get; set; }
        public int PendingExperiments { get; set; }
        public bool HasAttemptedExam { get; set; }
        public bool HasPassedExam { get; set; }
    }

    /// <summary>
    /// DTO for exam overview for a class
    /// </summary>
    public class TeacherExamOverviewDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string RoboticsLevelName { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public int DurationMinutes { get; set; }
        public int TotalQuestions { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal PassingPercentage { get; set; }
        public List<ClassExamSummaryDto> ClassSummaries { get; set; } = new();
    }
}
