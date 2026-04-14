using System.Security.Claims;
using FastEndpoints;

namespace SAPBOneWrapper.Api.Endpoints.Auth;

public class MeResponse
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public List<string> Roles { get; set; } = [];
}

public class MeEndpoint : EndpointWithoutRequest<MeResponse>
{
    public override void Configure()
    {
        Get("/auth/me");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = HttpContext.User;

        await Send.OkAsync(new MeResponse
        {
            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            UserName = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            Email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            FullName = user.FindFirstValue("fullName"),
            Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        });
    }
}
