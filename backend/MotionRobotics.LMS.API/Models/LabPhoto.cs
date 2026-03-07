namespace MotionRobotics.LMS.API.Models
{
    public class LabPhoto
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }

        /// <summary>Relative URL path, e.g. /lab-photos/1/abc123.jpg</summary>
        public string FilePath { get; set; } = string.Empty;

        public string? Caption { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public School? School { get; set; }
    }
}
