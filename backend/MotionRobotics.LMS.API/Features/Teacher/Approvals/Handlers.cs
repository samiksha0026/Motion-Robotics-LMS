using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Teacher;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Teacher.Approvals;

public class GetPendingApprovalsHandler : IQueryHandler<GetPendingApprovalsQuery, List<PendingApprovalDto>>
{
    private readonly ITeacherService _s;
    public GetPendingApprovalsHandler(ITeacherService s) => _s = s;
    public async Task<List<PendingApprovalDto>> Handle(GetPendingApprovalsQuery r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId) ?? throw new KeyNotFoundException("Teacher not found");
        return await _s.GetPendingApprovalsAsync(teacherId, r.ClassId);
    }
}

public class ApproveProgressHandler : ICommandHandler<ApproveProgressCommand, (bool Success, string Message)>
{
    private readonly ITeacherService _s;
    public ApproveProgressHandler(ITeacherService s) => _s = s;
    public async Task<(bool Success, string Message)> Handle(ApproveProgressCommand r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId) ?? throw new KeyNotFoundException("Teacher not found");
        return await _s.ApproveProgressAsync(teacherId, r.ProgressId, r.Data);
    }
}

public class BulkApproveProgressHandler : ICommandHandler<BulkApproveProgressCommand, (bool Success, string Message, int Processed)>
{
    private readonly ITeacherService _s;
    public BulkApproveProgressHandler(ITeacherService s) => _s = s;
    public async Task<(bool Success, string Message, int Processed)> Handle(BulkApproveProgressCommand r, CancellationToken ct)
    {
        var teacherId = await _s.GetTeacherIdByUserIdAsync(r.UserId) ?? throw new KeyNotFoundException("Teacher not found");
        return await _s.BulkApproveProgressAsync(teacherId, r.Data);
    }
}
