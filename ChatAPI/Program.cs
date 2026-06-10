using ChatAPI;
using ChatAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

string sqliteConnectionString = builder.Configuration.GetConnectionString("vector-store")?? throw new InvalidCastException("The connection string was not provided.");
builder.Services.AddSqliteCollection<string, DocChunk>("data-icm-chunks",sqliteConnectionString);

// Add services to the container.

builder.AddOllamaApiClient("chat-service").AddChatClient();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddOllamaResilienceHandlers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {   
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();