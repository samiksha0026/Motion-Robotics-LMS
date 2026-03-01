using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Teacher;

namespace MotionRobotics.LMS.API.Features.Teacher.Approvals;

public record GetPendingApprovalsQuery(string UserId, int? ClassId) : IQuery<List<PendingApprovalDto>>;
public record ApproveProgressCommand(string UserId, int ProgressId, ApprovalRequestDto Data) : ICommand<(bool Success, string Message)>;
public record BulkApproveProgressCommand(string UserId, BulkApprovalRequestDto Data) : ICommand<(bool Success, string Message, int Processed)>;
