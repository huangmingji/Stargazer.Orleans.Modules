var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Stargazer_Orleans_Users_Silo>("users-silo")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Stargazer_Orleans_ObjectStorage_Silo>("object-storage-silo")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Stargazer_Orleans_MessageManagement_Silo>("message-management-silo")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();
