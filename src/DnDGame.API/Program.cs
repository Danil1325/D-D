// This file is the entry point of the whole API. It runs once, at startup, and its
// only job is to configure two things:
//   1) which services are available for dependency injection (the "builder" section)
//   2) which middleware handles each incoming HTTP request (the "app" section)
// No game logic belongs here — this stays infrastructure-only, even after later phases.

using DnDGame.API.CompositionRoot;
using DnDGame.API.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// --- Register services (dependency injection container) ---

// CORS: the frontend (React/Vite) lives in a separate repo/origin from this API,
// and cookie auth requires the browser to send credentials cross-origin, which is
// only possible with an explicit (never wildcard) allowed-origin list plus
// AllowCredentials(). "Cors:AllowedOrigins" is empty by default (appsettings.json)
// so an unconfigured deployment denies all cross-origin requests rather than
// silently allowing everything; appsettings.Development.json supplies the Vite
// dev server's origin.
const string FrontendCorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Cookie authentication (Task: auth). Deliberately cookie-based, not JWT — see
// docs/ARCHITECTURE.md for the reasoning. HttpOnly keeps the cookie unreadable to
// JS (no XSS token theft); Secure/SameSite are relaxed only in Development, where
// the frontend runs over plain http on a different localhost port than the API.
//
// Data Protection (which encrypts/signs this cookie) needs no setup here: on a
// normal local Windows/Linux dev machine it auto-persists its key ring to disk
// per-user (e.g. DPAPI-protected files under %LOCALAPPDATA% on Windows), so
// cookies keep validating across restarts on this machine without any manual key
// configuration or User Secret. This only becomes a real concern for a production
// deployment with multiple instances or a non-persistent filesystem (e.g.
// containers): each instance would otherwise get its own key ring, so cookies
// from one instance would fail to validate on another, and a restart would
// invalidate every outstanding cookie. That needs an explicit shared/persisted
// key store (file share, Redis, Blob storage, etc.) at deployment time — a
// concrete hosting decision, not something to build speculatively now.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "DnDGame.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = builder.Environment.IsDevelopment()
            ? SameSiteMode.Lax
            : SameSiteMode.None;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        // This is a JSON API, not an MVC app with login pages — on missing/invalid
        // auth, return a plain status code instead of redirecting to a (nonexistent)
        // login page.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

// Lets controllers be found and routed to automatically.
builder.Services.AddControllers();

// These two together generate the Swagger/OpenAPI page you can browse to in a
// dev environment (e.g. https://localhost:xxxx/swagger) to see and try every
// endpoint without writing any frontend code. Extremely useful for Phase 5,
// where we manually test the whole game loop before a database exists.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DnD Game API",
        Version = "v1",
        Description = "Fantasy RPG Web Game backend. Endpoints are grouped by " +
                      "controller — Player, Cards, Deck, Battle, and Dice groups " +
                      "are added as each controller is implemented (Task 3.2)."
    });

    // Picks up <summary> XML doc comments on controllers/actions/DTOs and shows
    // them in the Swagger UI. Requires GenerateDocumentationFile in the .csproj
    // (see DnDGame.API.csproj) — safe to call even before any file has XML
    // comments; Swashbuckle just skips files that don't exist.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// --- Feature components, one extension method per owner (see CompositionRoot) ---
// Card-battle feature (Persoana 2), battle/turn system integration surface
// (Persoana 1), and the mock-data phase. The container is intentionally lazy:
// the unresolved "Person 1 seams" documented there don't prevent startup.
builder.Services.AddCardBattleServices();
builder.Services.AddBattleTurnSystemServices();
builder.Services.AddMockData();

var app = builder.Build();

// --- Configure the HTTP request pipeline (middleware) ---

// Registered first so it can catch exceptions from every later middleware and
// from Controllers — see Task 3.19 and ExceptionHandlingMiddleware's own comments.
app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Must run before UseAuthentication/UseAuthorization: the CORS preflight
// (OPTIONS) request carries no auth cookie, and the actual credentialed request
// needs the Access-Control-Allow-Origin/-Credentials headers this adds.
app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposes the implicit Program class to the test project (WebApplicationFactory<Program>
// needs a public type to boot the app in-memory for integration tests).
public partial class Program { }
