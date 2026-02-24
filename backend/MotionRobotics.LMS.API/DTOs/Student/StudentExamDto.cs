namespace MotionRobotics.LMS.API.DTOs.Student
{
    /// <summary>
    /// DTO for student exam eligibility check
    /// </summary>
    public class ExamEligibilityDto
    {
        public bool IsEligible { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalExperiments { get; set; }
        public int CompletedExperiments { get; set; }
        public int PendingExperiments { get; set; }
        public bool HasAttemptedExam { get; set; }
        public bool HasPassedExam { get; set; }
        public ExamPreviewDto? Exam { get; set; }
    }

    /// <summary>
    /// DTO for exam preview (before starting)
    /// </summary>
    public class ExamPreviewDto
    {
        public int ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public int TotalQuestions { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal PassingPercentage { get; set; }
        public decimal PassingMarks { get; set; }
        public string RoboticsLevelName { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
    }

    /// <summary>
    /// DTO for exam questions (student taking exam)
    /// </summary>
    public class ExamQuestionsDto
    {
        public int ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TotalQuestions { get; set; }
        public decimal TotalMarks { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime MustEndBy { get; set; }
        public List<StudentQuestionDto> Questions { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual question (without correct answer)
    /// </summary>
    public class StudentQuestionDto
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public decimal Marks { get; set; }
    }

    /// <summary>
    /// DTO for submitting exam answers
    /// </summary>
    public class ExamAnswerSubmitDto
    {
        public int ExamId { get; set; }
        public int TimeTakenSeconds { get; set; }
        public List<AnswerDto> Answers { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual answer
    /// </summary>
    public class AnswerDto
    {
        public int QuestionNumber { get; set; }
        public string Answer { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for exam result (student view after submission)
    /// </summary>
    public class StudentExamResultDto
    {
        public int ResultId { get; set; }
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string RoboticsLevelName { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public decimal ScoreObtained { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal Percentage { get; set; }
        public bool IsPassed { get; set; }
        public string ResultMessage { get; set; } = string.Empty;
        public int TimeTakenSeconds { get; set; }
        public string TimeTakenFormatted { get; set; } = string.Empty;
        public DateTime AttemptedAt { get; set; }
        public bool CertificateGenerated { get; set; }
        public int? CertificateId { get; set; }
        public List<QuestionResultDto>? QuestionResults { get; set; }
    }

    /// <summary>
    /// DTO for question result (showing correct/incorrect)
    /// </summary>
    public class QuestionResultDto
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string YourAnswer { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public decimal MarksObtained { get; set; }
        public decimal TotalMarks { get; set; }
        public string? Explanation { get; set; }
    }

    /// <summary>
    /// DTO for exam history
    /// </summary>
    public class ExamHistoryDto
    {
        public List<StudentExamResultDto> Results { get; set; } = new();
        public int TotalAttempts { get; set; }
        public int PassedCount { get; set; }
    }
}
