using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Teacher.Lab;

public class GetTeacherLabHandler : IQueryHandler<GetTeacherLabQuery, LabInfoDto?>
{
    private readonly ApplicationDbContext _context;
    private readonly ILabService _labService;

    public GetTeacherLabHandler(ApplicationDbContext context, ILabService labService)
    {
        _context = context;
        _labService = labService;
    }

    public async Task<LabInfoDto?> Handle(GetTeacherLabQuery request, CancellationToken cancellationToken)
    {
        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (teacher == null) return null;

        return await _labService.GetLabInfoAsync(teacher.SchoolId);
    }
}
