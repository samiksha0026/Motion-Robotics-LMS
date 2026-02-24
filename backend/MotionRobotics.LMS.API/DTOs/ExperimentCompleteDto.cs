namespace MotionRobotics.LMS.API.DTOs
{
    /// <summary>
    /// DTO for submitting experiment completion.
    /// </summary>
    public class ExperimentCompleteDto
    {
        public string? Notes { get; set; }  // Student's notes or observations
        public string? ImageUrl { get; set; }  // Photo proof of completed experiment
    }
}
