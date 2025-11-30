using Donclub.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    public record RequestOtpRequest(string PhoneNumber);
    public record VerifyOtpRequest(string PhoneNumber, string Code);
    public record RefreshRequest(string RefreshToken);

    [HttpPost("request-otp")]
    public async Task<ActionResult<RequestOtpResultDto>> RequestOtp([FromBody] RequestOtpRequest request, CancellationToken ct)
    {
        var result = await _auth.RequestOtpAsync(request.PhoneNumber, ct);
        return Ok(result);
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<AuthResultDto>> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _auth.VerifyOtpAsync(request.PhoneNumber, request.Code, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResultDto>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _auth.RefreshAsync(request.RefreshToken, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
