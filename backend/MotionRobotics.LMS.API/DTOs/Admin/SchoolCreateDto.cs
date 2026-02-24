using System.ComponentModel.DataAnnotations;

namespace MotionRobotics.LMS.API.DTOs.Admin
{
    public class SchoolCreateDto
    {
        [Required]
        [StringLength(200)]
        public required string SchoolName { get; set; }

        [Required]
        [StringLength(20)]
        public required string SchoolCode { get; set; }

        [Required]
        public required string Address { get; set; }

        [Required]
        [StringLength(100)]
        public required string City { get; set; }

        [Required]
        [StringLength(100)]
        public required string State { get; set; }

        [Required]
        [StringLength(10)]
        public required string Pincode { get; set; }

        [Required]
        [EmailAddress]
        public required string ContactEmail { get; set; }

        [Required]
        [Phone]
        public required string ContactPhone { get; set; }

        [StringLength(200)]
        public string? PrincipalName { get; set; }
    }
}
