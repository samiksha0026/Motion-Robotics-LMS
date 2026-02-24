namespace MotionRobotics.LMS.API.DTOs.Admin
{
    public class ClassCreateDto
    {
        public required string ClassName { get; set; }
        public required int SchoolId { get; set; }
        public int? RoboticsLevelId { get; set; }  // Optional: Assign level when creating class
    }
}
