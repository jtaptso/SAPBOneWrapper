using System.Net.Http.Json;

namespace SAPBOneWrapper.Web.Services;

public class AuthApiClient(HttpClient httpClient, JwtAuthenticationStateProvider authState)
{
    public async Task<LoginResult> LoginAsync(string userName, string password)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/login", new
        {
            UserName = userName,
            Password = password
        });

        if (!response.IsSuccessStatusCode)
            return new LoginResult { Success = false, Error = "Invalid username or password." };

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (result?.Token is null)
            return new LoginResult { Success = false, Error = "Login failed." };

        await authState.SetTokenAsync(result.Token);
        return new LoginResult { Success = true };
    }

    public async Task<RegisterResult> RegisterAsync(string userName, string email, string password, string? fullName)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            UserName = userName,
            Email = email,
            Password = password,
            FullName = fullName
        });

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return new RegisterResult { Success = false, Error = body };
        }

        return new RegisterResult { Success = true };
    }

    public async Task LogoutAsync()
    {
        await authState.ClearTokenAsync();
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiry { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
}

public class LoginResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public class RegisterResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}
