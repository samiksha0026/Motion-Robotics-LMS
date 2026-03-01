using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;
using MotionRobotics.LMS.API.Services;

namespace MotionRobotics.LMS.API.Features.Certificates;

public class VerifyCertificateHandler : IQueryHandler<VerifyCertificateQuery, CertificateVerificationDto>
{
    private readonly ICertificateService _certificateService;
    public VerifyCertificateHandler(ICertificateService certificateService)
        => _certificateService = certificateService;

    public Task<CertificateVerificationDto> Handle(VerifyCertificateQuery request, CancellationToken cancellationToken)
        => _certificateService.VerifyCertificateAsync(request.CertificateNumber);
}
