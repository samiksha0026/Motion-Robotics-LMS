using Microsoft.AspNetCore.Identity;

namespace MotionRobotics.LMS.API.Models
{
    /// <summary>
    /// Represents a school in the multi-school LMS system.
    /// Each school has its own admin, teachers, students, and level mappings.
    /// </summary>
    public class School
    {
        public int Id { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolCode { get; set; } = string.Empty;  // Unique code for the school
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }  // Used in certificates
        public string PrincipalName { get; set; } = string.Empty;

        // School Admin credentials (the person who manages this school's LMS)
        public string? SchoolAdminUserId { get; set; }  // Reference to AspNetUsers (SchoolAdmin role)

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public IdentityUser? SchoolAdminUser { get; set; }
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
        public ICollection<SchoolLevelMapping> SchoolLevelMappings { get; set; } = new List<SchoolLevelMapping>();
    }
}
