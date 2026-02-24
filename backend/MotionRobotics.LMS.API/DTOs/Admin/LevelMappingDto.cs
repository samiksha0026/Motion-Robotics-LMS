using System.ComponentModel.DataAnnotations;

namespace MotionRobotics.LMS.API.DTOs.Admin
{
    /// <summary>
    /// DTO for displaying level mapping information
    /// </summary>
    public class LevelMappingDto
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int RoboticsLevelId { get; set; }
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a single level mapping
    /// </summary>
    public class LevelMappingCreateDto
    {
        [Required]
        public int SchoolId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public int RoboticsLevelId { get; set; }

        [Required]
        public int AcademicYearId { get; set; }
    }

    /// <summary>
    /// DTO for bulk creating level mappings for a school
    /// </summary>
    public class BulkLevelMappingDto
    {
        [Required]
        public int SchoolId { get; set; }

        [Required]
        public int AcademicYearId { get; set; }

        /// <summary>
        /// List of class-to-level mappings
        /// </summary>
        [Required]
        public List<ClassLevelPair> Mappings { get; set; } = new();
    }

    public class ClassLevelPair
    {
        [Required]
        public int ClassId { get; set; }

        [Required]
        public int RoboticsLevelId { get; set; }
    }

    /// <summary>
    /// DTO for updating a level mapping (only level can be changed)
    /// </summary>
    public class LevelMappingUpdateDto
    {
        [Required]
        public int RoboticsLevelId { get; set; }
    }

    /// <summary>
    /// Response for school's level assignments
    /// </summary>
    public class SchoolLevelAssignmentsDto
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public int AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public List<LevelMappingDto> Mappings { get; set; } = new();
        public List<ClassSummaryDto> AvailableClasses { get; set; } = new();
    }

    public class ClassSummaryDto
    {
        public int Id { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public bool HasMapping { get; set; }
    }
}
