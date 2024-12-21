using Projects;

var builder = DistributedApplication.CreateBuilder(args);

//var username = builder.AddParameter("username", true);
//var password = builder.AddParameter("password", true);

//var postgres = builder.AddPostgres("postgres", username, password, 5432)
//    .WithPgAdmin()
//    .WithVolume("postgres-data-opencoremmo", "/var/lib/postgresql/data");

//var standaloneServer = builder.AddProject<NeoServer_Server_Standalone>("Standalone")
//    .WaitFor(postgres);

//var webApi = builder.AddProject<NeoServer_Web_API>("webApi")
//    .WaitFor(postgres);

//builder.AddProject<Projects.NeoServer_Server_WebAPI>("neoserver-server-webapi");

var loginServer = builder.AddProject<NeoServer_Server_Login>("LoginServer");

var gameServer = builder.AddProject<NeoServer_Server_Game>("GameServer")
    .WaitFor(loginServer);

builder.Build().Run();