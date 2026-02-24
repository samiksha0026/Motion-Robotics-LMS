using System.ComponentModel.DataAnnotations;

namespace MotionRobotics.LMS.API.DTOs.Student
{
    /// <summary>
    /// Full experiment details for student view
    /// </summary>
    public class StudentExperimentDto
    {
        public int ExperimentId { get; set; }
        public int SequenceOrder { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Objective { get; set; }
        public string? Components { get; set; }
        public string? Procedure { get; set; }
        public string? WiringDiagram { get; set; }
        public string? CircuitDiagram { get; set; }
        public string? CodeSnippet { get; set; }
        public string? DemoVideoUrl { get; set; }
        public string? SafetyNotes { get; set; }
        public int EstimatedMinutes { get; set; }
        public string Status { get; set; } = "locked";  // locked, available, in-progress, completed, approved
        public StudentExperimentProgressDto? Progress { get; set; }
    }

    /// <summary>
    /// Student's progress on a specific experiment
    /// </summary>
    public class StudentExperimentProgressDto
    {
        public int ProgressId { get; set; }
        public bool Completed { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? SubmissionNotes { get; set; }
        public string? SubmissionImageUrl { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? TeacherRemarks { get; set; }
        public string? ApprovedByTeacher { get; set; }
    }

    /// <summary>
    /// List of all experiments with status
    /// </summary>
    public class StudentExperimentsListDto
    {
        public CurrentLevelDto Level { get; set; } = new();
        public ProgressSummaryDto Progress { get; set; } = new();
        /// <summary>
        /// Number of experiments unlocked by teacher for this class
        /// </summary>
        public int UnlockedCount { get; set; }
        public List<StudentExperimentDto> Experiments { get; set; } = new();
    }

    /// <summary>
    /// Request to submit experiment completion
    /// </summary>
    public class ExperimentSubmissionDto
    {
        [StringLength(1000)]
        public string? SubmissionNotes { get; set; }

        [StringLength(500)]
        [Url]
        public string? SubmissionImageUrl { get; set; }
    }

    /// <summary>
    /// Response after submitting experiment
    /// </summary>
    public class ExperimentSubmissionResponseDto
    {
        public int ProgressId { get; set; }
        public int ExperimentId { get; set; }
        public string ExperimentTitle { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public ExperimentPreviewDto? NextExperiment { get; set; }
    }
}
