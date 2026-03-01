using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Students;

public class GetAllStudentsHandler : IQueryHandler<GetAllStudentsQuery, List<StudentResponseDto>>
{
    private readonly IAdminStudentService _s;
    public GetAllStudentsHandler(IAdminStudentService s) => _s = s;
    public Task<List<StudentResponseDto>> Handle(GetAllStudentsQuery r, CancellationToken ct) => _s.GetAllStudentsAsync();
}

public class GetStudentByIdHandler : IQueryHandler<GetStudentByIdQuery, StudentResponseDto?>
{
    private readonly IAdminStudentService _s;
    public GetStudentByIdHandler(IAdminStudentService s) => _s = s;
    public Task<StudentResponseDto?> Handle(GetStudentByIdQuery r, CancellationToken ct) => _s.GetStudentByIdAsync(r.Id);
}

public class GetStudentsBySchoolHandler : IQueryHandler<GetStudentsBySchoolQuery, List<StudentResponseDto>>
{
    private readonly IAdminStudentService _s;
    public GetStudentsBySchoolHandler(IAdminStudentService s) => _s = s;
    public Task<List<StudentResponseDto>> Handle(GetStudentsBySchoolQuery r, CancellationToken ct) => _s.GetStudentsBySchoolAsync(r.SchoolId);
}

public class GetStudentsByClassHandler : IQueryHandler<GetStudentsByClassQuery, List<StudentResponseDto>>
{
    private readonly IAdminStudentService _s;
    public GetStudentsByClassHandler(IAdminStudentService s) => _s = s;
    public Task<List<StudentResponseDto>> Handle(GetStudentsByClassQuery r, CancellationToken ct) => _s.GetStudentsByClassAsync(r.ClassId);
}

public class CreateStudentHandler : ICommandHandler<CreateStudentCommand, StudentResponseDto>
{
    private readonly IAdminStudentService _s;
    public CreateStudentHandler(IAdminStudentService s) => _s = s;
    public Task<StudentResponseDto> Handle(CreateStudentCommand r, CancellationToken ct) => _s.CreateStudentAsync(r.Data);
}

public class DeleteStudentHandler : ICommandHandler<DeleteStudentCommand, bool>
{
    private readonly IAdminStudentService _s;
    public DeleteStudentHandler(IAdminStudentService s) => _s = s;
    public Task<bool> Handle(DeleteStudentCommand r, CancellationToken ct) => _s.DeleteStudentAsync(r.Id);
}
