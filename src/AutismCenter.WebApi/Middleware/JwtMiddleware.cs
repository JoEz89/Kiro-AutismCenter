using AutismCenter.Application.Common.Interfaces;
using System.Security.Claims;

namespace AutismCenter.WebApi.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
    {
        var token = ExtractTokenFromHeader(context);

        if (!string.IsNullOrEmpty(token))
        {
            var validationResult = tokenService.ValidateToken(token);

            if (validationResult.IsValid && validationResult.UserId.HasValue)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, validationResult.UserId.Value.ToString()),
                };

                if (!string.IsNullOrEmpty(validationResult.Email))
                    claims.Add(new Claim(ClaimTypes.Email, validationResult.Email));

                if (!string.IsNullOrEmpty(validationResult.Role))
                    claims.Add(new Claim(ClaimTypes.Role, validationResult.Role));

                var identity = new ClaimsIdentity(claims, "jwt");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await _next(context);
    }

    private static string? ExtractTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authorizationHeader))
            return null;

        if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;

        return authorizationHeader["Bearer ".Length..].Trim();
    }
}