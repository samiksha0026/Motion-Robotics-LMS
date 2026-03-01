using MotionRobotics.LMS.API.Common.CQRS;
using MotionRobotics.LMS.API.DTOs.Student;

namespace MotionRobotics.LMS.API.Features.Student.Certificates;

public record GetStudentCertificatesQuery(string UserId) : IQuery<List<StudentCertificateDto>>;
public record GetStudentCertificateQuery(string UserId, int CertificateId) : IQuery<StudentCertificateDto?>;
