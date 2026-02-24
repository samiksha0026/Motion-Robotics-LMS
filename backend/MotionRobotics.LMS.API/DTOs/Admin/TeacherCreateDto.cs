namespace MotionRobotics.LMS.API.DTOs.Admin
{
    public class TeacherCreateDto
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string Password { get; set; }
        public required string PhoneNumber { get; set; }
        public required int SchoolId { get; set; }
        public List<int> ClassIds { get; set; } = new List<int>();  // Classes assigned to this teacher
    }
}
