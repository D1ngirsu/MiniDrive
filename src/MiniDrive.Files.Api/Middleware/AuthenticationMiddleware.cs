using MiniDrive.Clients.Identity;

namespace MiniDrive.Files.Api.Middleware;

/// <summary>
/// Middleware to extract and validate authentication token.
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IIdentityClient identityClient)
    {
        var authorization = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authorization))
        {
            var token = ExtractBearerToken(authorization);
            if (!string.IsNullOrEmpty(token))
            {
                var user = await identityClient.ValidateSessionAsync(token);
                if (user != null)
                {
                    // Store user info in HttpContext for use in controllers
                    context.Items["UserId"] = user.Id;
                    context.Items["User"] = user;
                }
            }
        }

        await _next(context);
    }

    private static string? ExtractBearerToken(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return null;
        }

        const string prefix = "Bearer ";
        return authorizationHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader[prefix.Length..].Trim()
            : authorizationHeader.Trim();
    }
}

