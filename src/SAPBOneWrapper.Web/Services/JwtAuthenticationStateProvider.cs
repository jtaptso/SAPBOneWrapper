using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SAPBOneWrapper.Web.Services;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private string? _token;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public string? Token => _token;

    public void SetToken(string token)
    {
        _token = token;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void ClearToken()
    {
        _token = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (string.IsNullOrEmpty(_token))
            return Task.FromResult(new AuthenticationState(_anonymous));

        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(_token))
            return Task.FromResult(new AuthenticationState(_anonymous));

        var jwt = handler.ReadJwtToken(_token);

        if (jwt.ValidTo < DateTime.UtcNow)
        {
            _token = null;
            return Task.FromResult(new AuthenticationState(_anonymous));
        }

        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }
}
