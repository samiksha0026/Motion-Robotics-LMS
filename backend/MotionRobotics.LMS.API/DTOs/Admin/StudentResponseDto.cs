namespace MotionRobotics.LMS.API.DTOs.Admin
{
    public class StudentResponseDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string RollNo { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public int ClassId { get; set; }
        public required string ClassName { get; set; }
        public int SchoolId { get; set; }
        public required string SchoolName { get; set; }
        // Robotics level determined by SchoolLevelMapping
        public int? AssignedLevelId { get; set; }
        public string? AssignedLevelName { get; set; }
        public int? AssignedLevelNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
