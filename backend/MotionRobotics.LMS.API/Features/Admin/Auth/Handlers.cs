using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Auth;

public class AdminLoginHandler : ICommandHandler<AdminLoginCommand, AdminLoginResponseDto?>
{
    private readonly IAdminAuthService _authService;
    public AdminLoginHandler(IAdminAuthService authService) => _authService = authService;

    public Task<AdminLoginResponseDto?> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
        => _authService.LoginAsync(request.Request);
}
