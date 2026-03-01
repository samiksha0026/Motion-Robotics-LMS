using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Teacher.Auth;

public class TeacherLoginHandler : ICommandHandler<TeacherLoginCommand, object?>
{
    private readonly TeacherAuthService _s;
    public TeacherLoginHandler(TeacherAuthService s) => _s = s;
    public async Task<object?> Handle(TeacherLoginCommand r, CancellationToken ct) => await _s.LoginAsync(r.Data);
}
