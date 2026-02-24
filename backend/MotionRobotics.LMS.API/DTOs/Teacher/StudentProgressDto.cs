using System.ComponentModel.DataAnnotations;

namespace MotionRobotics.LMS.API.DTOs.Teacher
{
    /// <summary>
    /// Student progress entry for teacher view
    /// </summary>
    public class StudentProgressDto
    {
        public int ProgressId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public int ExperimentId { get; set; }
        public int ExperimentSequence { get; set; }
        public string ExperimentTitle { get; set; } = string.Empty;
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
    /// Pending approval entry
    /// </summary>
    public class PendingApprovalDto
    {
        public int ProgressId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int ExperimentId { get; set; }
        public int ExperimentSequence { get; set; }
        public string ExperimentTitle { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
        public string? SubmissionNotes { get; set; }
        public string? SubmissionImageUrl { get; set; }
    }

    /// <summary>
    /// Request to approve or reject a student's progress
    /// </summary>
    public class ApprovalRequestDto
    {
        [Required]
        public bool Approve { get; set; }

        [StringLength(500)]
        public string? TeacherRemarks { get; set; }
    }

    /// <summary>
    /// Bulk approval request
    /// </summary>
    public class BulkApprovalRequestDto
    {
        [Required]
        public List<int> ProgressIds { get; set; } = new();

        [Required]
        public bool Approve { get; set; }

        [StringLength(500)]
        public string? TeacherRemarks { get; set; }
    }

    /// <summary>
    /// Student's detailed progress view
    /// </summary>
    public class StudentDetailedProgressDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int RoboticsLevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public int TotalExperiments { get; set; }
        public int CompletedExperiments { get; set; }
        public int ApprovedExperiments { get; set; }
        public decimal ProgressPercentage { get; set; }
        public List<ExperimentStatusDto> Experiments { get; set; } = new();
    }

    /// <summary>
    /// Single experiment status for a student
    /// </summary>
    public class ExperimentStatusDto
    {
        public int ExperimentId { get; set; }
        public int SequenceOrder { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int EstimatedMinutes { get; set; }
        public string Status { get; set; } = "locked";  // locked, available, in-progress, completed, approved
        public DateTime? CompletedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? SubmissionNotes { get; set; }
        public string? SubmissionImageUrl { get; set; }
        public string? TeacherRemarks { get; set; }
    }
}
