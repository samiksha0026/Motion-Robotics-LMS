namespace MotionRobotics.LMS.API.DTOs.Admin
{
    public class StudentUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string RollNo { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public int SchoolId { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
    }
}
