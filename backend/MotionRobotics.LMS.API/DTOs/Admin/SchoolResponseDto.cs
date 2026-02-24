namespace MotionRobotics.LMS.API.DTOs.Admin
{
    public class SchoolResponseDto
    {
        public int Id { get; set; }
        public required string SchoolName { get; set; }
        public required string SchoolCode { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string Pincode { get; set; }
        public required string ContactEmail { get; set; }
        public required string ContactPhone { get; set; }
        public string? LogoUrl { get; set; }
        public string? PrincipalName { get; set; }
        public string? LoginUsername { get; set; }  // Only returned on creation
        public string? LoginPassword { get; set; }  // Only returned on creation
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int StudentCount { get; set; }
        public int ClassCount { get; set; }
        public int TeacherCount { get; set; }
    }
}
