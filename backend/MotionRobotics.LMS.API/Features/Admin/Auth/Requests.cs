using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Auth;

// ── Commands ─────────────────────────────────────────────────────────────────
public record AdminLoginCommand(AdminLoginRequestDto Request) : ICommand<AdminLoginResponseDto?>;
