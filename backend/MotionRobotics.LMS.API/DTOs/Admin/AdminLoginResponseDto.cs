namespace MotionRobotics.LMS.API.DTOs.Admin
{
    public class AdminLoginResponseDto
    {
        public required string Token { get; set; }
        public required string AdminEmail { get; set; }
        public required List<string> Roles { get; set; }
        public required DateTime ExpiresAt { get; set; }
    }
}
