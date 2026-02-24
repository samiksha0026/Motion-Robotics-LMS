using System.ComponentModel.DataAnnotations;

namespace MotionRobotics.LMS.API.DTOs.Admin
{
    /// <summary>
    /// DTO for displaying academic year information
    /// </summary>
    public class AcademicYearDto
    {
        public int Id { get; set; }
        public string YearName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public int TotalMappings { get; set; }
        public int TotalStudentProgress { get; set; }
    }

    /// <summary>
    /// DTO for creating/updating academic year
    /// </summary>
    public class AcademicYearCreateDto
    {
        [Required]
        [StringLength(20, MinimumLength = 7)]
        public string YearName { get; set; } = string.Empty;  // e.g., "2025-2026"

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool SetAsCurrent { get; set; } = false;
    }
}
