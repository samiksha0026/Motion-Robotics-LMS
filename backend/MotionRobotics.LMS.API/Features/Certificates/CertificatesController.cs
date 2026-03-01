using MediatR;
using Microsoft.AspNetCore.Mvc;
using MotionRobotics.LMS.API.DTOs.Admin;

namespace MotionRobotics.LMS.API.Features.Certificates;

/// <summary>Public endpoint to verify certificate authenticity (no auth required)</summary>
[ApiController]
[Route("api/certificates")]
[Tags("Certificates - Public")]
public class CertificatesController : ControllerBase
{
    private readonly IMediator _mediator;
    public CertificatesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Verify a certificate by its certificate number</summary>
    [HttpGet("verify/{certificateNumber}")]
    [ProducesResponseType(typeof(CertificateVerificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CertificateVerificationDto>> VerifyCertificate(string certificateNumber)
    {
        var result = await _mediator.Send(new VerifyCertificateQuery(certificateNumber));
        return Ok(result);
    }
}
