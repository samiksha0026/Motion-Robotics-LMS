using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Services.Admin
{
    public interface IAdminAuthService
    {
        Task<AdminLoginResponseDto?> LoginAsync(AdminLoginRequestDto request);
    }
}
