var builder = DistributedApplication.CreateBuilder(args);

var userSilo = builder.AddProject<Projects.Stargazer_Orleans_Users_Silo>("users-silo")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Stargazer_Orleans_ObjectStorage_Silo>("object-storage-silo")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(userSilo)
    .WaitFor(userSilo);

builder.AddProject<Projects.Stargazer_Orleans_MessageManagement_Silo>("message-management-silo")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(userSilo)
    .WaitFor(userSilo);

builder.Build().Run();
