namespace MotionRobotics.LMS.API.DTOs.Admin
{
    public class AttendanceCreateDto
    {
        public required int StudentId { get; set; }
        public required int ClassId { get; set; }
        public required DateTime AttendanceDate { get; set; }
        public required bool IsPresent { get; set; }
        public string? Remarks { get; set; }
    }
}
