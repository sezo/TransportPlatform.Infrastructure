using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TransportPlatform.Infrastructure.Common.Auth;

// ── Extensions ────────────────────────────────────────────────────────────────

public static class AuthExtensions
{
    /// <summary>
    /// Registers JWT bearer auth (validates M2M token from gateway),
    /// permission policy provider, and UserContext accessor.
    ///
    /// Usage: builder.Services.AddTransportAuth(builder.Configuration);
    /// </summary>
    public static IServiceCollection AddTransportAuth(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = config["Keycloak:Authority"];
                options.TokenValidationParameters.ValidateAudience = false;
                options.RequireHttpsMetadata = false;
            });

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<UserContext>();

        return services;
    }

    /// <summary>
    /// Registers UserContextMiddleware in the pipeline.
    /// Must be called after UseAuthentication() and before UseAuthorization().
    ///
    /// Usage: app.UseTransportUserContext();
    /// </summary>
    public static IApplicationBuilder UseTransportUserContext(
        this IApplicationBuilder app) =>
        app.UseMiddleware<UserContextMiddleware>();
}

// ── Middleware ────────────────────────────────────────────────────────────────

/// <summary>
/// Reads user context headers injected by YARP gateway.
/// Builds ClaimsPrincipal from trusted headers so [Authorize] attributes work.
/// Services never validate user JWT directly — gateway handles that.
/// </summary>
public class UserContextMiddleware(
    RequestDelegate next,
    ILogger<UserContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.Request.Headers["X-User-Id"].FirstOrDefault();
        var userRoles = context.Request.Headers["X-User-Roles"]
            .FirstOrDefault()
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
        var correlationId = context.Request.Headers["X-Correlation-Id"]
            .FirstOrDefault() ?? Guid.NewGuid().ToString();

        if (userId is not null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId)
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));

                if (role.StartsWith("permission:"))
                    claims.Add(new Claim("permission", role["permission:".Length..].Trim()));
            }

            context.User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, "GatewayHeaders"));
        }

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        logger.LogDebug("Request {CorrelationId} user {UserId}",
            correlationId, userId ?? "anonymous");

        await next(context);
    }
}

// ── UserContext ───────────────────────────────────────────────────────────────

/// <summary>
/// Strongly typed accessor for current user identity.
/// Inject into controllers and handlers instead of reading HttpContext directly.
/// </summary>
public class UserContext(IHttpContextAccessor accessor)
{
    public Guid? UserId =>
        accessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
            is string id ? Guid.Parse(id) : null;

    public string[] Roles =>
        accessor.HttpContext?.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray() ?? [];

    public string CorrelationId =>
        accessor.HttpContext?.Items["CorrelationId"] as string
            ?? string.Empty;

    public bool IsInRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}

// ── Permission policy provider ────────────────────────────────────────────────

/// <summary>
/// Dynamically generates authorization policies from permission strings.
/// [Authorize(Policy = "permission:ticket:read")] → RequireClaim("permission", "ticket:read")
/// [Authorize(Policy = "permission:ticket:read|ticket:write")] → OR logic
/// No manual policy registration needed.
/// </summary>
public class PermissionPolicyProvider(
    IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    private const string PermissionPrefix = "permission:";
    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(PermissionPrefix))
            return await _fallback.GetPolicyAsync(policyName);

        var permissions = policyName[PermissionPrefix.Length..]
            .Split('|', StringSplitOptions.RemoveEmptyEntries);

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser();

        if (permissions.Length == 1)
            policy.RequireClaim("permission", permissions[0].Trim());
        else
        {
            var list = permissions.Select(p => p.Trim()).ToArray();
            policy.RequireAssertion(ctx =>
                list.Any(p => ctx.User.HasClaim("permission", p)));
        }

        return policy.Build();
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallback.GetFallbackPolicyAsync();
}
