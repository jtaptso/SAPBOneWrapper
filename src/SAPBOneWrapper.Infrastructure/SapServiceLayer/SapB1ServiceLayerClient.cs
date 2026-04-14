using System.Net.Http.Json;
using System.Text.Json;
using SAPBOneWrapper.Domain.Entities;
using SAPBOneWrapper.Domain.Enums;
using SAPBOneWrapper.Domain.Interfaces;
using SAPBOneWrapper.Infrastructure.SapServiceLayer.Models;

namespace SAPBOneWrapper.Infrastructure.SapServiceLayer;

public class SapB1ServiceLayerClient(HttpClient httpClient) : ISapB1ServiceLayerClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<BusinessPartner?> GetBusinessPartnerAsync(string cardCode, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync($"/b1s/v1/BusinessPartners('{cardCode}')", ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessOrThrowAsync(response, ct);

        var sapModel = await response.Content.ReadFromJsonAsync<SapBusinessPartnerModel>(JsonOptions, ct);
        return sapModel is null ? null : MapToDomain(sapModel);
    }

    public async Task<IReadOnlyList<BusinessPartner>> GetBusinessPartnersAsync(
        string? filter = null, int top = 20, int skip = 0, CancellationToken ct = default)
    {
        var allPartners = new List<BusinessPartner>();
        var url = BuildQueryUrl(filter, top, skip);

        do
        {
            var response = await httpClient.GetAsync(url, ct);
            await EnsureSuccessOrThrowAsync(response, ct);

            var result = await response.Content.ReadFromJsonAsync<SapQueryResponse<SapBusinessPartnerModel>>(JsonOptions, ct);
            if (result?.Value is null) break;

            allPartners.AddRange(result.Value.Select(MapToDomain));
            url = result.NextLink;
        }
        while (url is not null);

        return allPartners;
    }

    public async Task<BusinessPartner> CreateBusinessPartnerAsync(BusinessPartner bp, CancellationToken ct = default)
    {
        var sapModel = MapToSapModel(bp);
        var response = await httpClient.PostAsJsonAsync("/b1s/v1/BusinessPartners", sapModel, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var created = await response.Content.ReadFromJsonAsync<SapBusinessPartnerModel>(JsonOptions, ct);
        return created is not null ? MapToDomain(created) : bp;
    }

    public async Task UpdateBusinessPartnerAsync(string cardCode, BusinessPartner bp, CancellationToken ct = default)
    {
        var sapModel = MapToSapModel(bp);
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/b1s/v1/BusinessPartners('{cardCode}')")
        {
            Content = JsonContent.Create(sapModel)
        };

        var response = await httpClient.SendAsync(request, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task DeleteBusinessPartnerAsync(string cardCode, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"/b1s/v1/BusinessPartners('{cardCode}')", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    private static string BuildQueryUrl(string? filter, int top, int skip)
    {
        var url = $"/b1s/v1/BusinessPartners?$top={top}&$skip={skip}";
        url += "&$select=CardCode,CardName,CardType,Phone1,EmailAddress,Address,City,Country,ZipCode,Currency,MaxCommitment,FederalTaxID,Valid,FreeText";

        if (!string.IsNullOrWhiteSpace(filter))
            url += $"&$filter={Uri.EscapeDataString(filter)}";

        return url;
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(ct);

        try
        {
            var error = JsonSerializer.Deserialize<SapErrorResponse>(body, JsonOptions);
            if (error?.Error?.Message?.Value is not null)
                throw new HttpRequestException(
                    $"SAP B1 Error ({error.Error.Code}): {error.Error.Message.Value}",
                    null, response.StatusCode);
        }
        catch (JsonException) { }

        throw new HttpRequestException(
            $"SAP B1 request failed ({(int)response.StatusCode}): {body}",
            null, response.StatusCode);
    }

    private static BusinessPartner MapToDomain(SapBusinessPartnerModel model)
    {
        return new BusinessPartner
        {
            CardCode = model.CardCode,
            CardName = model.CardName ?? string.Empty,
            CardType = model.CardType switch
            {
                "cCustomer" => CardType.Customer,
                "cSupplier" => CardType.Supplier,
                "cLead" => CardType.Lead,
                _ => CardType.Customer
            },
            Phone = model.Phone,
            Email = model.Email,
            Address = model.Address,
            City = model.City,
            Country = model.Country,
            PostCode = model.PostCode,
            Currency = model.Currency,
            CreditLimit = model.CreditLimit,
            TaxId = model.TaxId,
            Active = model.Active == "tYES",
            Remarks = model.Remarks
        };
    }

    private static SapBusinessPartnerModel MapToSapModel(BusinessPartner bp)
    {
        return new SapBusinessPartnerModel
        {
            CardCode = bp.CardCode,
            CardName = bp.CardName,
            CardType = bp.CardType switch
            {
                CardType.Customer => "cCustomer",
                CardType.Supplier => "cSupplier",
                CardType.Lead => "cLead",
                _ => "cCustomer"
            },
            Phone = bp.Phone,
            Email = bp.Email,
            Address = bp.Address,
            City = bp.City,
            Country = bp.Country,
            PostCode = bp.PostCode,
            Currency = bp.Currency,
            CreditLimit = bp.CreditLimit,
            TaxId = bp.TaxId,
            Active = bp.Active ? "tYES" : "tNO",
            Remarks = bp.Remarks
        };
    }
}
