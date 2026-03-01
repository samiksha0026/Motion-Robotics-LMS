using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Attendance;

public class GetAllAttendanceHandler : IQueryHandler<GetAllAttendanceQuery, List<AttendanceResponseDto>>
{
    private readonly IAttendanceService _s;
    public GetAllAttendanceHandler(IAttendanceService s) => _s = s;
    public Task<List<AttendanceResponseDto>> Handle(GetAllAttendanceQuery r, CancellationToken ct)
        => _s.GetAllAttendanceAsync(r.SchoolId, r.ClassId, r.StartDate, r.EndDate);
}

public class GetAttendanceByIdHandler : IQueryHandler<GetAttendanceByIdQuery, AttendanceResponseDto?>
{
    private readonly IAttendanceService _s;
    public GetAttendanceByIdHandler(IAttendanceService s) => _s = s;
    public Task<AttendanceResponseDto?> Handle(GetAttendanceByIdQuery r, CancellationToken ct) => _s.GetAttendanceByIdAsync(r.Id);
}

public class GetAttendanceSummaryHandler : IQueryHandler<GetAttendanceSummaryQuery, AttendanceSummaryDto>
{
    private readonly IAttendanceService _s;
    public GetAttendanceSummaryHandler(IAttendanceService s) => _s = s;
    public Task<AttendanceSummaryDto> Handle(GetAttendanceSummaryQuery r, CancellationToken ct)
        => _s.GetAttendanceSummaryAsync(r.SchoolId, r.ClassId, r.StartDate, r.EndDate);
}

public class GetStudentAttendanceReportHandler : IQueryHandler<GetStudentAttendanceReportQuery, StudentAttendanceReportDto>
{
    private readonly IAttendanceService _s;
    public GetStudentAttendanceReportHandler(IAttendanceService s) => _s = s;
    public Task<StudentAttendanceReportDto> Handle(GetStudentAttendanceReportQuery r, CancellationToken ct)
        => _s.GetStudentAttendanceReportAsync(r.StudentId, r.StartDate, r.EndDate);
}

public class GetClassAttendanceReportHandler : IQueryHandler<GetClassAttendanceReportQuery, ClassAttendanceReportDto>
{
    private readonly IAttendanceService _s;
    public GetClassAttendanceReportHandler(IAttendanceService s) => _s = s;
    public Task<ClassAttendanceReportDto> Handle(GetClassAttendanceReportQuery r, CancellationToken ct)
        => _s.GetClassAttendanceReportAsync(r.ClassId, r.StartDate, r.EndDate);
}

public class GetSchoolAttendanceReportHandler : IQueryHandler<GetSchoolAttendanceReportQuery, SchoolAttendanceReportDto>
{
    private readonly IAttendanceService _s;
    public GetSchoolAttendanceReportHandler(IAttendanceService s) => _s = s;
    public Task<SchoolAttendanceReportDto> Handle(GetSchoolAttendanceReportQuery r, CancellationToken ct)
        => _s.GetSchoolAttendanceReportAsync(r.SchoolId, r.StartDate, r.EndDate);
}

public class DeleteAttendanceHandler : ICommandHandler<DeleteAttendanceCommand, bool>
{
    private readonly IAttendanceService _s;
    public DeleteAttendanceHandler(IAttendanceService s) => _s = s;
    public Task<bool> Handle(DeleteAttendanceCommand r, CancellationToken ct) => _s.DeleteAttendanceAsync(r.Id);
}
