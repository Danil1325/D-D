// This file is the entry point of the whole API. It runs once, at startup, and its
// only job is to configure two things:
//   1) which services are available for dependency injection (the "builder" section)
//   2) which middleware handles each incoming HTTP request (the "app" section)
// No game logic belongs here — this stays infrastructure-only, even after later phases.

using DnDGame.API.Middleware;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using DnDGame.MockData.Services;

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

// --- Mock data (Phase 2 equivalent for the card-battle feature) ---
//
// Only the pieces explicitly approved as "safe to implement now" are registered
// here: the shared in-memory store, the pre-existing IEnemyRepository, and the
// mock current-player abstraction. Repositories for Player/Card/Deck are NOT
// registered yet — those Domain entities don't exist until the team confirms the
// Player-vs-PlayerCharacter decision and Person 2's Card/Deck contracts.
//
// CreateSeededStore() is used directly (instead of an IServiceCollection
// extension method like AddMockData()) because MockData is a plain class library
// with no dependency on Microsoft.Extensions.DependencyInjection — adding that
// package reference is a call for whoever owns that project's dependency list,
// not something to introduce as a side effect of this feature's DI wiring.
var mockDataStore = MockDataBootstrapper.CreateSeededStore();
builder.Services.AddSingleton(mockDataStore);
builder.Services.AddScoped<IEnemyRepository, MockEnemyRepository>();
builder.Services.AddScoped<ICurrentPlayerService, MockCurrentPlayerService>();

// --- Cross-cutting infrastructure ---
builder.Services.AddSingleton<IErrorCodeHttpMapper, ErrorCodeHttpMapper>();

// Still to come once contracts are confirmed:
//   Person 2: IPlayerRepository/MockPlayerRepository (pending Player decision),
//             ICardRepository/MockCardRepository, IDeckRepository/MockDeckRepository
//   Person 1: IBattleStateStore/InMemoryBattleStateStore (needs the real BattleState type)
//   Both:     BattleService, DiceService, DeckService's engine calls
// and Phase 7 will replace the Mock* repositories with DataAccessLayer's EF Core
// ones — only the two lines above (and their DataAccessLayer equivalents) change.

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
