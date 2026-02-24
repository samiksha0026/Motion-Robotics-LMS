namespace MotionRobotics.LMS.API.DTOs.Admin
{
    public class StudentCreateDto
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string Password { get; set; }
        public required string RollNo { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public required int ClassId { get; set; }
        public required int SchoolId { get; set; }
        // Note: Robotics level is now determined by SchoolLevelMapping (School + Class + Academic Year)
    }
}
