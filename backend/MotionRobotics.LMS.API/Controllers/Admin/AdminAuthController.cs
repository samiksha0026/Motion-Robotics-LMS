using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IAdminAuthService _authService;

        public AdminAuthController(IAdminAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(result);
        }
    }
}
