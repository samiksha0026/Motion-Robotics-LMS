using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Admin.Certificates;

public class GetAllCertificatesHandler : IQueryHandler<GetAllCertificatesQuery, CertificateListDto>
{
    private readonly ICertificateService _s;
    public GetAllCertificatesHandler(ICertificateService s) => _s = s;
    public Task<CertificateListDto> Handle(GetAllCertificatesQuery r, CancellationToken ct) => _s.GetAllCertificatesAsync(r.SchoolId, r.RoboticsLevelId, r.AcademicYearId);
}

public class GetCertificateByIdHandler : IQueryHandler<GetCertificateByIdQuery, CertificateDetailDto?>
{
    private readonly ICertificateService _s;
    public GetCertificateByIdHandler(ICertificateService s) => _s = s;
    public Task<CertificateDetailDto?> Handle(GetCertificateByIdQuery r, CancellationToken ct) => _s.GetCertificateByIdAsync(r.CertificateId);
}

public class GetCertificateHtmlHandler : IQueryHandler<GetCertificateHtmlQuery, string>
{
    private readonly ICertificateService _s;
    public GetCertificateHtmlHandler(ICertificateService s) => _s = s;
    public Task<string> Handle(GetCertificateHtmlQuery r, CancellationToken ct) => _s.GenerateCertificateHtmlAsync(r.CertificateId);
}

public class RegenerateCertificateHandler : ICommandHandler<RegenerateCertificateCommand, bool>
{
    private readonly ICertificateService _s;
    public RegenerateCertificateHandler(ICertificateService s) => _s = s;
    public Task<bool> Handle(RegenerateCertificateCommand r, CancellationToken ct) => _s.RegenerateCertificateAsync(r.CertificateId, r.CustomTitle);
}
