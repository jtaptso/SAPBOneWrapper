using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SAPBOneWrapper.Infrastructure.SapServiceLayer.Models;

namespace SAPBOneWrapper.Infrastructure.SapServiceLayer;

public class SapB1SessionHandler : DelegatingHandler
{
    private readonly SapB1Options _options;
    private string? _sessionId;
    private DateTime _sessionExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _loginLock = new(1, 1);

    public SapB1SessionHandler(IOptions<SapB1Options> options)
    {
        _options = options.Value;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Don't add session cookie to login requests
        if (!IsLoginRequest(request))
        {
            await EnsureSessionAsync(request, cancellationToken);
            request.Headers.TryAddWithoutValidation("Cookie", $"B1SESSION={_sessionId}");
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Auto-relogin on 401
        if (response.StatusCode == HttpStatusCode.Unauthorized && !IsLoginRequest(request))
        {
            _sessionId = null;
            await EnsureSessionAsync(request, cancellationToken);

            // Clone the request with new session
            var retry = await CloneRequestAsync(request);
            retry.Headers.TryAddWithoutValidation("Cookie", $"B1SESSION={_sessionId}");
            response = await base.SendAsync(retry, cancellationToken);
        }

        return response;
    }

    private bool IsLoginRequest(HttpRequestMessage request)
    {
        return request.RequestUri?.AbsolutePath.EndsWith("/Login", StringComparison.OrdinalIgnoreCase) == true;
    }

    private async Task EnsureSessionAsync(HttpRequestMessage originalRequest, CancellationToken ct)
    {
        if (_sessionId is not null && DateTime.UtcNow < _sessionExpiry)
            return;

        await _loginLock.WaitAsync(ct);
        try
        {
            if (_sessionId is not null && DateTime.UtcNow < _sessionExpiry)
                return;

            var loginRequest = new HttpRequestMessage(HttpMethod.Post,
                new Uri(new Uri(_options.ServiceLayerUrl), "/b1s/v1/Login"));

            loginRequest.Content = JsonContent.Create(new SapLoginRequest
            {
                CompanyDB = _options.CompanyDB,
                UserName = _options.UserName,
                Password = _options.Password
            });

            var response = await base.SendAsync(loginRequest, ct);
            response.EnsureSuccessStatusCode();

            var loginResponse = await response.Content.ReadFromJsonAsync<SapLoginResponse>(ct);
            _sessionId = loginResponse?.SessionId
                ?? throw new InvalidOperationException("SAP B1 login failed: no session ID returned.");
            _sessionExpiry = DateTime.UtcNow.AddMinutes(_options.SessionTimeoutMinutes - 1);
        }
        finally
        {
            _loginLock.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);
            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }
}
