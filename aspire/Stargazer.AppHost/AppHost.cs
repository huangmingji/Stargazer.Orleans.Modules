using Stargazer.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.CreateRedisResource();
var postgresql = builder.CreatePostgresDatabaseResource();
var keycloak = builder.CreateKeycloakResource()
    .WithReference(postgresql)
    .WaitFor(postgresql.GetPostgresServerResource());
var seq = builder.CreateSeqResource();

var orleansServer = builder.AddProject<Projects.Stargazer_Orleans_Template_Silo>("orleans-server")
    // .WithExternalHttpEndpoints()
    // .WithHttpHealthCheck("/health")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(seq)
    .WaitFor(seq)
    .WithReference(keycloak)
    .WaitFor(keycloak);

builder.AddProject<Projects.Stargazer_Orleans_Template_Client>("orleans-client")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(orleansServer)
    .WaitFor(orleansServer);

builder.AddProject<Projects.Stargazer_Orleans_Template_Host>("api-service")
    .WithExternalHttpEndpoints()
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(orleansServer)
    .WaitFor(orleansServer);

builder.Build().Run();
