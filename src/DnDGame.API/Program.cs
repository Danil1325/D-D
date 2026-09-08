// This file is the entry point of the whole API. It runs once, at startup, and its
// only job is to configure two things:
//   1) which services are available for dependency injection (the "builder" section)
//   2) which middleware handles each incoming HTTP request (the "app" section)
// No game logic belongs here — this stays infrastructure-only, even after later phases.

var builder = WebApplication.CreateBuilder(args);

// --- Register services (dependency injection container) ---

// Lets controllers be found and routed to automatically.
builder.Services.AddControllers();

// These two together generate the Swagger/OpenAPI page you can browse to in a
// dev environment (e.g. https://localhost:xxxx/swagger) to see and try every
// endpoint without writing any frontend code. Extremely useful for Phase 5,
// where we manually test the whole game loop before a database exists.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Phase 2 will add something like:
//   builder.Services.AddMockData();
// and Phase 7 will replace it with:
//   builder.Services.AddDataAccessLayer(builder.Configuration);
// Phase 3 will add:
//   builder.Services.AddBusinessLayer();
// Nothing else in this file needs to change when that swap happens.

var app = builder.Build();

// --- Configure the HTTP request pipeline (middleware) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
