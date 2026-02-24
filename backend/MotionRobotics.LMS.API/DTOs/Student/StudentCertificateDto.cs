namespace MotionRobotics.LMS.API.DTOs.Student
{
    /// <summary>
    /// Full certificate details
    /// </summary>
    public class StudentCertificateDto
    {
        public int CertificateId { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public string? CertificateFileUrl { get; set; }
        public int ExperimentsCompleted { get; set; }
    }

    /// <summary>
    /// Student profile info
    /// </summary>
    public class StudentProfileDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RollNo { get; set; } = string.Empty;
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime JoinedAt { get; set; }
        public List<LevelProgressHistoryDto> LevelHistory { get; set; } = new();
    }

    /// <summary>
    /// Progress history for a level
    /// </summary>
    public class LevelProgressHistoryDto
    {
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public int ExperimentsCompleted { get; set; }
        public int TotalExperiments { get; set; }
        public bool HasCertificate { get; set; }
        public DateTime? CertificateDate { get; set; }
    }
}
