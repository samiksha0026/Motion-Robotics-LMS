using Microsoft.EntityFrameworkCore;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Student.Lab;

public class GetStudentLabHandler : IQueryHandler<GetStudentLabQuery, LabInfoDto?>
{
    private readonly ApplicationDbContext _context;
    private readonly ILabService _labService;

    public GetStudentLabHandler(ApplicationDbContext context, ILabService labService)
    {
        _context = context;
        _labService = labService;
    }

    public async Task<LabInfoDto?> Handle(GetStudentLabQuery request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);

        if (student == null) return null;

        return await _labService.GetLabInfoAsync(student.SchoolId);
    }
}
