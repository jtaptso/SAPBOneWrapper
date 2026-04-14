namespace SAPBOneWrapper.Infrastructure.SapServiceLayer;

public class SapB1Options
{
    public const string SectionName = "SapB1";

    public string ServiceLayerUrl { get; set; } = string.Empty;
    public string CompanyDB { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int SessionTimeoutMinutes { get; set; } = 30;
}
