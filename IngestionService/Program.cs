using IngestionService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddOllamaApiClient("embedding-service").AddEmbeddingGenerator();

var sqliteConnectionString = builder.Configuration.GetConnectionString("vector-store");
builder.Services.AddSqliteVectorStore(_=>sqliteConnectionString?? throw new InvalidCastException("Could not convert the SQLite connection string to vector store"));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();