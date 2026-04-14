using System.Net.Http.Headers;
using System.Net.Http.Json;
using SAPBOneWrapper.Application.DTOs;

namespace SAPBOneWrapper.Web.Services;

public class BusinessPartnerApiClient(HttpClient httpClient, JwtAuthenticationStateProvider authState)
{
    private void SetAuthHeader()
    {
        if (authState.Token is not null)
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<PagedResultDto<BusinessPartnerDto>?> GetAllAsync(
        string? search = null, int page = 1, int pageSize = 20)
    {
        SetAuthHeader();
        var url = $"/api/business-partners?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        return await httpClient.GetFromJsonAsync<PagedResultDto<BusinessPartnerDto>>(url);
    }

    public async Task<BusinessPartnerDto?> GetByCodeAsync(string cardCode)
    {
        SetAuthHeader();
        return await httpClient.GetFromJsonAsync<BusinessPartnerDto>($"/api/business-partners/{cardCode}");
    }

    public async Task<HttpResponseMessage> CreateAsync(CreateBusinessPartnerDto dto)
    {
        SetAuthHeader();
        return await httpClient.PostAsJsonAsync("/api/business-partners", dto);
    }

    public async Task<HttpResponseMessage> UpdateAsync(string cardCode, UpdateBusinessPartnerDto dto)
    {
        SetAuthHeader();
        return await httpClient.PutAsJsonAsync($"/api/business-partners/{cardCode}", dto);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string cardCode)
    {
        SetAuthHeader();
        return await httpClient.DeleteAsync($"/api/business-partners/{cardCode}");
    }

    public async Task<SyncResult?> SyncAsync()
    {
        SetAuthHeader();
        var response = await httpClient.PostAsync("/api/business-partners/sync", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SyncResult>();
    }
}

public class SyncResult
{
    public int SyncedCount { get; set; }
    public string Message { get; set; } = string.Empty;
}
