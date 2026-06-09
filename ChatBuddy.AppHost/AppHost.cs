var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllamaLocal("ollama-service");
var chatModel = ollama.AddModel("chat-service", "llama3.2:1b");
var embeddings = ollama.AddModel("embedding-service", "all-minilm");

/*
builder
    .AddContainer("open-webui", "ghcr.io/open-webui/open-webui", "main")
    .WithHttpEndpoint(port: 3000, targetPort: 8080, name: "http")
    .WithEnvironment("OLLAMA_BASE_URL", ollama.GetEndpoint("http"))
    .WithLifetime(ContainerLifetime.Persistent)
    .WaitFor(ollama);
 */
builder.AddProject<Projects.ChatAPI>("ChatAPI")
    .WithReference(chatModel)
    .WaitFor(chatModel);

builder.AddProject<Projects.IngestionService>("IngestionService")
    .WithReference(embeddings)
    .WaitFor(embeddings);

builder.Build().Run();
