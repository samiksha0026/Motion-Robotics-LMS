using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Controllers.Teacher
{
    [ApiController]
    [Route("api/teacher/auth")]
    public class TeacherAuthController : ControllerBase
    {
        private readonly TeacherAuthService _authService;

        public TeacherAuthController(TeacherAuthService authService)
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
                return Unauthorized(new { message = "Invalid email or password or not a teacher" });

            return Ok(result);
        }
    }
}
