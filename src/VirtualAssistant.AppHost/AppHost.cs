using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> apiService = builder.AddProject<VirtualAssistant_Api>("assistant-api")
    .WithHttpHealthCheck("/health");

builder.AddProject<VirtualAssistant_Web_Server>("assistant-web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
