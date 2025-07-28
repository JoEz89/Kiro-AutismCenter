using AutismCenter.Application.Features.Authentication.Commands.RegisterUser;
using AutismCenter.Application.Features.Authentication.Commands.VerifyEmail;
using AutismCenter.Application.Features.Authentication.Commands.GoogleLogin;
using AutismCenter.Application.Features.Authentication.Commands.ForgotPassword;
using AutismCenter.Application.Features.Authentication.Commands.ResetPassword;
using AutismCenter.Application.Features.Authentication.Commands.Login;
using AutismCenter.Application.Features.Authentication.Commands.RefreshToken;
using AutismCenter.Application.Features.Authentication.Commands.Logout;
using AutismCenter.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutismCenter.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    /// <param name="command">Login credentials</param>
    /// <returns>Authentication result with tokens</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="command">Registration details</param>
    /// <returns>Registration result</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterUserResponse>> Register([FromBody] RegisterUserCommand command)
    {
        try
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="command">Refresh token</param>
    /// <returns>New authentication result with tokens</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResult>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        try
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Logout user and revoke refresh token
    /// </summary>
    /// <param name="command">Refresh token to revoke</param>
    /// <returns>Logout result</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LogoutResponse>> Logout([FromBody] LogoutCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Verify email address using verification token (POST)
    /// </summary>
    /// <param name="command">Email verification token</param>
    /// <returns>Verification result</returns>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(VerifyEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(VerifyEmailResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerifyEmailResponse>> VerifyEmail([FromBody] VerifyEmailCommand command)
    {
        var result = await Mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Verify email address using verification token (GET)
    /// </summary>
    /// <param name="token">Email verification token</param>
    /// <returns>Verification result</returns>
    [HttpGet("verify-email")]
    [ProducesResponseType(typeof(VerifyEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(VerifyEmailResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerifyEmailResponse>> VerifyEmailGet([FromQuery] string token)
    {
        var command = new VerifyEmailCommand(token);
        var result = await Mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Authenticate user with Google OAuth
    /// </summary>
    /// <param name="command">Google OAuth token</param>
    /// <returns>Authentication result</returns>
    [HttpPost("google-login")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GoogleLogin([FromBody] GoogleLoginCommand command)
    {
        try
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }
}