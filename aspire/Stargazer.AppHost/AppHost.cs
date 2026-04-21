var builder = DistributedApplication.CreateBuilder(args);

var userSilo = builder.AddProject<Projects.Stargazer_Orleans_Users_Silo>("users-silo")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

var objectStorageSilo = builder.AddProject<Projects.Stargazer_Orleans_ObjectStorage_Silo>("object-storage-silo")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(userSilo)
    .WaitFor(userSilo);

var messageManagementSilo = builder.AddProject<Projects.Stargazer_Orleans_MessageManagement_Silo>("message-management-silo")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(userSilo)
    .WaitFor(userSilo);

var frontend = builder.AddNpmApp("frontend", "../../front-end/management", "dev")
    .WithHttpEndpoint(port: 3001, env: "PORT")
    .WithEnvironment(context =>
    {
        context.EnvironmentVariables["NODE_ENV"] = "development";
    })
    .WithReference(userSilo)
    .WaitFor(userSilo)
    .WithReference(objectStorageSilo)
    .WaitFor(objectStorageSilo)
    .WithReference(messageManagementSilo)
    .WaitFor(messageManagementSilo);

builder.Build().Run();
