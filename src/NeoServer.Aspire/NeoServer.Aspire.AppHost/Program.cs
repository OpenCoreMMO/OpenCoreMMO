var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("username", secret: true);
var password = builder.AddParameter("password", secret: true);

var postgres = builder.AddPostgres("postgres", username, password, port: 5432)
                    .WithPgAdmin()
                    .WithVolume("postgres-data-opencoremmo", "/var/lib/postgresql/data");

var standaloneServer = builder.AddProject<Projects.NeoServer_Server_Standalone>("Standalone")
                            .WaitFor(postgres);

var webApi = builder.AddProject<Projects.NeoServer_Web_API>("webApi")
                            .WaitFor(postgres);

builder.Build().Run();