using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Student;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Student.Certificates;

public class GetStudentCertificatesHandler : IQueryHandler<GetStudentCertificatesQuery, List<StudentCertificateDto>>
{
    private readonly IStudentService _s;
    public GetStudentCertificatesHandler(IStudentService s) => _s = s;
    public async Task<List<StudentCertificateDto>> Handle(GetStudentCertificatesQuery r, CancellationToken ct)
    {
        var studentId = await _s.GetStudentIdByUserIdAsync(r.UserId);
        return studentId.HasValue ? await _s.GetCertificatesAsync(studentId.Value) : new List<StudentCertificateDto>();
    }
}

public class GetStudentCertificateHandler : IQueryHandler<GetStudentCertificateQuery, StudentCertificateDto?>
{
    private readonly IStudentService _s;
    public GetStudentCertificateHandler(IStudentService s) => _s = s;
    public async Task<StudentCertificateDto?> Handle(GetStudentCertificateQuery r, CancellationToken ct)
    {
        var studentId = await _s.GetStudentIdByUserIdAsync(r.UserId);
        return studentId.HasValue ? await _s.GetCertificateAsync(studentId.Value, r.CertificateId) : null;
    }
}
