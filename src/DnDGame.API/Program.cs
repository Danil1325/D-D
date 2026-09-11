// This file is the entry point of the whole API. It runs once, at startup, and its
// only job is to configure two things:
//   1) which services are available for dependency injection (the "builder" section)
//   2) which middleware handles each incoming HTTP request (the "app" section)
// No game logic belongs here — this stays infrastructure-only, even after later phases.

using DnDGame.API.CompositionRoot;
using DnDGame.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- Register services (dependency injection container) ---

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

app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposes the implicit Program class to the test project (WebApplicationFactory<Program>
// needs a public type to boot the app in-memory for integration tests).
public partial class Program { }
