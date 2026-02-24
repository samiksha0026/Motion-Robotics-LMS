namespace MotionRobotics.LMS.API.DTOs.Admin
{
    public class TeacherResponseDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string PhoneNumber { get; set; }
        public int SchoolId { get; set; }
        public required string SchoolName { get; set; }
        public List<ClassResponseDto> Classes { get; set; } = new List<ClassResponseDto>();
        public DateTime CreatedAt { get; set; }
    }
}
