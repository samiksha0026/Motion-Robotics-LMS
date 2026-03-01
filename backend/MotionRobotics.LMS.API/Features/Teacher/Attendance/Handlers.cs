using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Teacher.Attendance;

public class RecordAttendanceHandler : ICommandHandler<RecordAttendanceCommand, AttendanceResponseDto>
{
    private readonly IAttendanceService _a; private readonly ITeacherService _t;
    public RecordAttendanceHandler(IAttendanceService a, ITeacherService t) { _a = a; _t = t; }
    public async Task<AttendanceResponseDto> Handle(RecordAttendanceCommand r, CancellationToken ct)
    {
        var teacherId = await _t.GetTeacherIdByUserIdAsync(r.UserId) ?? throw new KeyNotFoundException("Teacher not found");
        return await _a.RecordAttendanceAsync(teacherId, r.Data);
    }
}

public class RecordBulkAttendanceHandler : ICommandHandler<RecordBulkAttendanceCommand, BulkAttendanceResponseDto>
{
    private readonly IAttendanceService _a; private readonly ITeacherService _t;
    public RecordBulkAttendanceHandler(IAttendanceService a, ITeacherService t) { _a = a; _t = t; }
    public async Task<BulkAttendanceResponseDto> Handle(RecordBulkAttendanceCommand r, CancellationToken ct)
    {
        var teacherId = await _t.GetTeacherIdByUserIdAsync(r.UserId) ?? throw new KeyNotFoundException("Teacher not found");
        return await _a.RecordBulkAttendanceAsync(teacherId, r.Data);
    }
}

public class GetAttendanceByIdHandler : IQueryHandler<GetAttendanceByIdQuery, AttendanceResponseDto?>
{
    private readonly IAttendanceService _s;
    public GetAttendanceByIdHandler(IAttendanceService s) => _s = s;
    public Task<AttendanceResponseDto?> Handle(GetAttendanceByIdQuery r, CancellationToken ct) => _s.GetAttendanceByIdAsync(r.Id);
}

public class GetClassAttendanceHandler : IQueryHandler<GetClassAttendanceQuery, List<AttendanceResponseDto>>
{
    private readonly IAttendanceService _s;
    public GetClassAttendanceHandler(IAttendanceService s) => _s = s;
    public Task<List<AttendanceResponseDto>> Handle(GetClassAttendanceQuery r, CancellationToken ct) => _s.GetClassAttendanceAsync(r.ClassId);
}

public class GetClassAttendanceByDateHandler : IQueryHandler<GetClassAttendanceByDateQuery, List<AttendanceResponseDto>>
{
    private readonly IAttendanceService _s;
    public GetClassAttendanceByDateHandler(IAttendanceService s) => _s = s;
    public Task<List<AttendanceResponseDto>> Handle(GetClassAttendanceByDateQuery r, CancellationToken ct) => _s.GetClassAttendanceByDateAsync(r.ClassId, r.Date);
}

public class GetStudentAttendanceHandler : IQueryHandler<GetStudentAttendanceQuery, List<AttendanceResponseDto>>
{
    private readonly IAttendanceService _s;
    public GetStudentAttendanceHandler(IAttendanceService s) => _s = s;
    public Task<List<AttendanceResponseDto>> Handle(GetStudentAttendanceQuery r, CancellationToken ct) => _s.GetStudentAttendanceAsync(r.StudentId);
}

public class GetStudentAttendanceReportHandler : IQueryHandler<GetStudentAttendanceReportQuery, StudentAttendanceReportDto>
{
    private readonly IAttendanceService _s;
    public GetStudentAttendanceReportHandler(IAttendanceService s) => _s = s;
    public Task<StudentAttendanceReportDto> Handle(GetStudentAttendanceReportQuery r, CancellationToken ct) => _s.GetStudentAttendanceReportAsync(r.StudentId, r.StartDate, r.EndDate);
}

public class UpdateAttendanceHandler : ICommandHandler<UpdateAttendanceCommand, AttendanceResponseDto>
{
    private readonly IAttendanceService _s;
    public UpdateAttendanceHandler(IAttendanceService s) => _s = s;
    public Task<AttendanceResponseDto> Handle(UpdateAttendanceCommand r, CancellationToken ct) => _s.UpdateAttendanceAsync(r.Id, r.Data);
}

public class DeleteAttendanceHandler : ICommandHandler<DeleteAttendanceCommand, bool>
{
    private readonly IAttendanceService _s;
    public DeleteAttendanceHandler(IAttendanceService s) => _s = s;
    public Task<bool> Handle(DeleteAttendanceCommand r, CancellationToken ct) => _s.DeleteAttendanceAsync(r.Id);
}
