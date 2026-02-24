using System.Text.Json;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.DTOs.Student;

namespace MotionRobotics.LMS.API.Helpers
{
    /// <summary>
    /// Helper class for evaluating exam answers
    /// </summary>
    public static class ExamEvaluator
    {
        /// <summary>
        /// Evaluates student answers against correct answers
        /// </summary>
        public static ExamEvaluationResult Evaluate(
            List<QuestionDto> questions,
            List<AnswerDto> answers)
        {
            var result = new ExamEvaluationResult
            {
                TotalMarks = questions.Sum(q => q.Marks),
                QuestionResults = new List<QuestionResultDto>()
            };

            foreach (var question in questions)
            {
                var answer = answers.FirstOrDefault(a => a.QuestionNumber == question.QuestionNumber);
                var studentAnswer = answer?.Answer?.Trim() ?? string.Empty;
                var correctAnswer = question.CorrectAnswer?.Trim() ?? string.Empty;

                bool isCorrect = IsAnswerCorrect(question.QuestionType, studentAnswer, correctAnswer);
                decimal marksObtained = isCorrect ? question.Marks : 0;

                result.ScoreObtained += marksObtained;
                result.QuestionResults.Add(new QuestionResultDto
                {
                    QuestionNumber = question.QuestionNumber,
                    QuestionText = question.QuestionText,
                    YourAnswer = studentAnswer,
                    CorrectAnswer = correctAnswer,
                    IsCorrect = isCorrect,
                    MarksObtained = marksObtained,
                    TotalMarks = question.Marks,
                    Explanation = question.Explanation
                });
            }

            result.Percentage = result.TotalMarks > 0
                ? Math.Round((result.ScoreObtained / result.TotalMarks) * 100, 2)
                : 0;

            return result;
        }

        /// <summary>
        /// Checks if answer is correct based on question type
        /// </summary>
        private static bool IsAnswerCorrect(string questionType, string studentAnswer, string correctAnswer)
        {
            if (string.IsNullOrWhiteSpace(studentAnswer)) return false;

            return questionType.ToUpperInvariant() switch
            {
                "MCQ" => studentAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase),
                "TRUEFALSE" => studentAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase),
                "SHORTANSWER" => studentAnswer.Trim().Equals(correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase),
                _ => studentAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase)
            };
        }

        /// <summary>
        /// Parses questions JSON from exam
        /// </summary>
        public static List<QuestionDto> ParseQuestionsJson(string? questionsJson)
        {
            if (string.IsNullOrWhiteSpace(questionsJson))
                return new List<QuestionDto>();

            try
            {
                return JsonSerializer.Deserialize<List<QuestionDto>>(questionsJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<QuestionDto>();
            }
            catch
            {
                return new List<QuestionDto>();
            }
        }

        /// <summary>
        /// Serializes questions to JSON
        /// </summary>
        public static string SerializeQuestions(List<QuestionCreateDto> questions)
        {
            var questionDtos = questions.Select((q, index) => new QuestionDto
            {
                QuestionNumber = q.QuestionNumber > 0 ? q.QuestionNumber : index + 1,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options,
                CorrectAnswer = q.CorrectAnswer,
                Marks = q.Marks,
                Explanation = q.Explanation
            }).ToList();

            return JsonSerializer.Serialize(questionDtos, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Serializes answers to JSON
        /// </summary>
        public static string SerializeAnswers(List<AnswerDto> answers)
        {
            return JsonSerializer.Serialize(answers, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Formats time in seconds to readable string
        /// </summary>
        public static string FormatTime(int seconds)
        {
            var time = TimeSpan.FromSeconds(seconds);
            if (time.Hours > 0)
                return $"{time.Hours}h {time.Minutes}m {time.Seconds}s";
            if (time.Minutes > 0)
                return $"{time.Minutes}m {time.Seconds}s";
            return $"{time.Seconds}s";
        }
    }

    /// <summary>
    /// Result of exam evaluation
    /// </summary>
    public class ExamEvaluationResult
    {
        public decimal ScoreObtained { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal Percentage { get; set; }
        public List<QuestionResultDto> QuestionResults { get; set; } = new();
    }
}
