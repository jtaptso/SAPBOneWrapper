using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SAPBOneWrapper.Infrastructure.Identity;

namespace SAPBOneWrapper.Api.Endpoints.Auth;

public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiry { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
}

public class LoginEndpoint(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration)
    : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await userManager.FindByNameAsync(req.UserName);
        if (user is null || !await userManager.CheckPasswordAsync(user, req.Password))
        {
            await Send.UnauthorizedAsync();
            return;
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        await Send.OkAsync(new LoginResponse
        {
            Token = token.Token,
            Expiry = token.Expiry,
            UserName = user.UserName!,
            FullName = user.FullName
        });
    }

    private (string Token, DateTime Expiry) GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("fullName", user.FullName ?? string.Empty)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var expiryMinutes = int.Parse(configuration["Jwt:ExpiryInMinutes"] ?? "60");
        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }
}
