namespace MotionRobotics.LMS.API.DTOs.Admin
{
    /// <summary>
    /// DTO for displaying robotics level information
    /// </summary>
    public class RoboticsLevelDto
    {
        public int Id { get; set; }
        public int LevelNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? SyllabusUrl { get; set; }
        public bool IsActive { get; set; }
        public int TotalExperiments { get; set; }
    }

    /// <summary>
    /// Detailed DTO with experiments list
    /// </summary>
    public class RoboticsLevelDetailDto : RoboticsLevelDto
    {
        public List<ExperimentSummaryDto> Experiments { get; set; } = new();
    }

    public class ExperimentSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int SequenceOrder { get; set; }
        public int EstimatedMinutes { get; set; }
        public bool IsActive { get; set; }
    }
}
