using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace SAPBOneWrapper.Web.Services;

public class JwtAuthenticationStateProvider(IJSRuntime js) : AuthenticationStateProvider
{
    private const string StorageKey = "auth_token";
    private string? _token;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public string? Token => _token;

    public async Task SetTokenAsync(string token)
    {
        _token = token;
        try { await js.InvokeVoidAsync("sessionStorage.setItem", StorageKey, token); }
        catch { /* JS unavailable during SSR */ }
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task ClearTokenAsync()
    {
        _token = null;
        try { await js.InvokeVoidAsync("sessionStorage.removeItem", StorageKey); }
        catch { /* JS unavailable during SSR */ }
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (string.IsNullOrEmpty(_token))
        {
            try { _token = await js.InvokeAsync<string?>("sessionStorage.getItem", StorageKey); }
            catch { /* JS unavailable during SSR — return anonymous */ }
        }

        if (string.IsNullOrEmpty(_token))
            return new AuthenticationState(_anonymous);

        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(_token))
            return new AuthenticationState(_anonymous);

        var jwt = handler.ReadJwtToken(_token);

        if (jwt.ValidTo < DateTime.UtcNow)
        {
            _token = null;
            try { await js.InvokeVoidAsync("sessionStorage.removeItem", StorageKey); }
            catch { }
            return new AuthenticationState(_anonymous);
        }

        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }
}
