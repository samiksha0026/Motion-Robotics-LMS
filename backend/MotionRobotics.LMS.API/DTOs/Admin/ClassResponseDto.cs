namespace MotionRobotics.LMS.API.DTOs.Admin
{
    public class ClassResponseDto
    {
        public int Id { get; set; }
        public required string ClassName { get; set; }
        public int SchoolId { get; set; }
        public required string SchoolName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int StudentCount { get; set; }
        public int? RoboticsLevelId { get; set; }
        public string? RoboticsLevelName { get; set; }
        public int? RoboticsLevelNumber { get; set; }
        public string? TeacherName { get; set; } // Primary assigned teacher
        public List<string>? TeacherNames { get; set; } // All assigned teachers
    }
}
