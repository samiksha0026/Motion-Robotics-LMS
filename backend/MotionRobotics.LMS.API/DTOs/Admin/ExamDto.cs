namespace MotionRobotics.LMS.API.DTOs.Admin
{
    /// <summary>
    /// DTO for creating a new exam
    /// </summary>
    public class ExamCreateDto
    {
        public int RoboticsLevelId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; } = 30;
        public decimal PassingPercentage { get; set; } = 40;
        public List<QuestionCreateDto> Questions { get; set; } = new();
    }

    /// <summary>
    /// DTO for creating a question
    /// </summary>
    public class QuestionCreateDto
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "MCQ"; // MCQ, TrueFalse, ShortAnswer
        public List<string> Options { get; set; } = new(); // For MCQ
        public string CorrectAnswer { get; set; } = string.Empty;
        public decimal Marks { get; set; } = 1;
        public string? Explanation { get; set; }
    }

    /// <summary>
    /// DTO for updating an exam
    /// </summary>
    public class ExamUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? DurationMinutes { get; set; }
        public decimal? PassingPercentage { get; set; }
        public bool? IsActive { get; set; }
        public List<QuestionCreateDto>? Questions { get; set; }
    }

    /// <summary>
    /// DTO for exam response
    /// </summary>
    public class ExamResponseDto
    {
        public int Id { get; set; }
        public int RoboticsLevelId { get; set; }
        public string RoboticsLevelName { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public int TotalQuestions { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal PassingPercentage { get; set; }
        public decimal PassingMarks { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for exam with questions (admin view)
    /// </summary>
    public class ExamDetailDto : ExamResponseDto
    {
        public List<QuestionDto> Questions { get; set; } = new();
    }

    /// <summary>
    /// DTO for question display
    /// </summary>
    public class QuestionDto
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public string CorrectAnswer { get; set; } = string.Empty;
        public decimal Marks { get; set; }
        public string? Explanation { get; set; }
    }

    /// <summary>
    /// DTO for exam list response
    /// </summary>
    public class ExamListDto
    {
        public List<ExamResponseDto> Exams { get; set; } = new();
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// DTO for exam result (admin view)
    /// </summary>
    public class ExamResultResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentRollNo { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
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
        public DateTime AttemptedAt { get; set; }
        public bool CertificateGenerated { get; set; }
        public int? CertificateId { get; set; }
    }

    /// <summary>
    /// DTO for exam results list
    /// </summary>
    public class ExamResultsListDto
    {
        public List<ExamResultResponseDto> Results { get; set; } = new();
        public int TotalCount { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public decimal AveragePercentage { get; set; }
    }

    /// <summary>
    /// DTO for exam statistics
    /// </summary>
    public class ExamStatisticsDto
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public decimal PassRate { get; set; }
        public decimal AverageScore { get; set; }
        public decimal HighestScore { get; set; }
        public decimal LowestScore { get; set; }
        public double AverageTimeTakenMinutes { get; set; }
    }
}
