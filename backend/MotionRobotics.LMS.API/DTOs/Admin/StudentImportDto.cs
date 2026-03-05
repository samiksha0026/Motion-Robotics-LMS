namespace MotionRobotics.LMS.API.DTOs.Admin
{
    /// <summary>
    /// Returned after an Excel bulk-import operation.
    /// </summary>
    public class StudentImportResultDto
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<StudentImportErrorDto> Errors { get; set; } = new();
    }

    /// <summary>
    /// Describes a single row that failed during import.
    /// </summary>
    public class StudentImportErrorDto
    {
        /// <summary>Excel row number (header = 1, data starts at 2).</summary>
        public int Row { get; set; }
        public string Error { get; set; } = string.Empty;
    }
}
