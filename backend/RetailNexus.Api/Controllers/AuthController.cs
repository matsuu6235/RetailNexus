using Microsoft.AspNetCore.Mvc;
using RetailNexus.Application.Features.Auth.Login;

namespace RetailNexus.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginHandler _handler;

    public AuthController(LoginHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        try
        {
            var res = await _handler.HandleAsync(command, ct);
            return Ok(res);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }
}