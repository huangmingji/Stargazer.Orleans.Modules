var builder = DistributedApplication.CreateBuilder(args);

var usersSilo = builder.AddProject<Projects.Stargazer_Orleans_Users_Silo>("users-silo")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();
