using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Admin.Certificates;

public record GetAllCertificatesQuery(int? SchoolId, int? RoboticsLevelId, int? AcademicYearId) : IQuery<CertificateListDto>;
public record GetCertificateByIdQuery(int CertificateId) : IQuery<CertificateDetailDto?>;
public record GetCertificateHtmlQuery(int CertificateId) : IQuery<string>;
public record RegenerateCertificateCommand(int CertificateId, string? CustomTitle) : ICommand<bool>;
