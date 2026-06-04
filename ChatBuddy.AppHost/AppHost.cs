var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ChatAPI>("ChatAPI");
builder.Build().Run();