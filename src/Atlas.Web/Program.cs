using Atlas.Application;
using Atlas.Infrastructure;
using Atlas.Web;
using Atlas.Web.Branding;
using Atlas.Web.Components;
using Atlas.Web.Components.Account;
using Atlas.Web.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/access-denied";
    options.Cookie.Name = "atlas.auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
builder.Services.AddAuthorization();

// Persist Data Protection keys (auth cookies, antiforgery) so sign-ins survive restarts.
// Point DataProtection:KeysPath at persistent storage in containers / App Service.
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
{
    builder.Services.AddDataProtection()
        .SetApplicationName("Atlas")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

// Azure App Service and containers terminate TLS at a proxy.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Emit Arabic (and other non-Latin) text as-is instead of &#x...; escapes; it is still HTML-encoded safely.
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

builder.Services.AddMudServices();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<UseCases>();
builder.Services.AddSingleton<BrandingService>();

var app = builder.Build();

// ---------- Pipeline ----------
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseSecurityHeaders();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

if (!app.Configuration.GetValue<bool>("DisableHttpsRedirection"))
{
    app.UseHttpsRedirection();
}

// Uploaded files (e.g. the logo) live outside wwwroot so they survive redeployments.
var uploadsRoot = app.Configuration.GetUploadsRootPath(app.Environment);
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = app.Configuration["FileStorage:RequestPath"] ?? "/uploads",
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapHealthEndpoints();
app.MapSeoEndpoints();
app.MapAccountEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.Services.SeedDatabaseAsync();

await app.RunAsync();
