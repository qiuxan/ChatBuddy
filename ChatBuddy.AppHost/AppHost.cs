var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllamaLocal("ollama-service");
var chatModel = ollama.AddModel("chat-service", "llama3.2:1b");
var embeddings = ollama.AddModel("embedding-service", "all-minilm");

var vectorStore = builder.AddSqlite("vector-store");

/*
builder
    .AddContainer("open-webui", "ghcr.io/open-webui/open-webui", "main")
    .WithHttpEndpoint(port: 3000, targetPort: 8080, name: "http")
    .WithEnvironment("OLLAMA_BASE_URL", ollama.GetEndpoint("http"))
    .WithLifetime(ContainerLifetime.Persistent)
    .WaitFor(ollama);
 */

var vectorDbPath = $"Data Source={builder.AppHostDirectory}/../chatbuddy-vectors.db;Cache=Shared;Mode=ReadWriteCreate";

builder.AddProject<Projects.ChatAPI>("ChatAPI")
    .WithReference(chatModel)
    .WithReference(vectorStore)
    .WithEnvironment("ConnectionStrings__vector-store", vectorDbPath)
    .WaitFor(chatModel);

builder.AddProject<Projects.IngestionService>("IngestionService")
    .WithReference(embeddings)
    .WithReference(vectorStore)
    .WithEnvironment("ConnectionStrings__vector-store", vectorDbPath)
    .WaitFor(embeddings)
    .WaitFor(vectorStore);

builder.Build().Run();
