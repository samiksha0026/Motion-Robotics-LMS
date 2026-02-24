namespace MotionRobotics.LMS.API.DTOs
{
    /// <summary>
    /// DTO for submitting exam answers and score.
    /// </summary>
    public class ExamSubmissionDto
    {
        public decimal Score { get; set; }  // Score obtained
        public string? AnswersJson { get; set; }  // JSON of answers submitted
        public int TimeTakenSeconds { get; set; }  // Time taken to complete exam
    }
}
