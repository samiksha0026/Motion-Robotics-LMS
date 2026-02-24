namespace MotionRobotics.LMS.API.DTOs.Admin
{
    public class AttendanceResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public required string StudentName { get; set; }
        public int ClassId { get; set; }
        public required string ClassName { get; set; }
        public int TeacherId { get; set; }
        public required string TeacherName { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool IsPresent { get; set; }
        public string? Remarks { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
