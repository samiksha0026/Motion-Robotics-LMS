using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public interface IStudentImportService
    {
        /// <summary>
        /// Parses the uploaded Excel file and bulk-creates students for the given school.
        /// Valid rows are committed; invalid rows are collected and returned in the result.
        /// </summary>
        Task<StudentImportResultDto> ImportFromExcelAsync(IFormFile file, int schoolId);

        /// <summary>
        /// Generates a ready-to-fill Excel template (.xlsx) with correct headers and a sample row.
        /// Returns the file as raw bytes so the controller can stream it to the client.
        /// </summary>
        byte[] GenerateTemplate();
    }
}
