using MediatR;
using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Certificates;

// ── Queries ───────────────────────────────────────────────────────────────────
public record VerifyCertificateQuery(string CertificateNumber) : IQuery<CertificateVerificationDto>;
