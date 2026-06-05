var builder = DistributedApplication.CreateBuilder(args);

var ollama=builder.AddOllama("ollama-service").WithDataVolume();
var chatModel= ollama.AddModel("chat-service", "llama3.2:1b");
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

builder.Build().Run();