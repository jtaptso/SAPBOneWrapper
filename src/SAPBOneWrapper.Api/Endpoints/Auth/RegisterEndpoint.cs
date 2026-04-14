using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using SAPBOneWrapper.Infrastructure.Identity;

namespace SAPBOneWrapper.Api.Endpoints.Auth;

public class RegisterRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
}

public class RegisterResponse
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class RegisterEndpoint(UserManager<ApplicationUser> userManager)
    : Endpoint<RegisterRequest, RegisterResponse>
{
    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            UserName = req.UserName,
            Email = req.Email,
            FullName = req.FullName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, req.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                AddError(error.Description);

            await Send.ErrorsAsync();
            return;
        }

        await Send.ResponseAsync(new RegisterResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email
        }, StatusCodes.Status201Created);
    }
}
