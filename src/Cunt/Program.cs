global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;



using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using NetCord.Hosting.Gateway;

using Cunt;

// var builder = Host.CreateDefaultBuilder(args);
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<RoleEnforcer>();

builder.Services.AddDiscordGateway();

await builder.Build().RunAsync();


