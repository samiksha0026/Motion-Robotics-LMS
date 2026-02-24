namespace MotionRobotics.LMS.API.DTOs
{
    public class StudentCreateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RollNo { get; set; } = string.Empty;
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public int ClassId { get; set; }
        public int SchoolId { get; set; }
        // Note: Robotics level determined by SchoolLevelMapping
    }
}
