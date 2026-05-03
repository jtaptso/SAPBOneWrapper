using Microsoft.AspNetCore.Components.Authorization;
using SAPBOneWrapper.Web.Components;
using SAPBOneWrapper.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Auth state
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options => options.LoginPath = "/login");
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>(sp =>
    (JwtAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// API clients
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001";
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<BusinessPartnerApiClient>();

builder.Services.AddScoped(sp =>
{
    var handler = new HttpClientHandler();
    var client = new HttpClient(handler) { BaseAddress = new Uri(apiBase) };
    return client;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous(); // HTTP-level auth disabled; [Authorize] enforced by AuthorizeRouteView via JwtAuthenticationStateProvider

app.Run();
