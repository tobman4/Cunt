global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;



using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using NetCord.Hosting.Gateway;
using NetCord.Gateway;

using Cunt;

DotEnv.LoadEnvFile("./.env");

var builder = Host.CreateApplicationBuilder(args);


// builder.Services.AddHostedService<RoleEnforcer>();
builder.Services.AddDiscordGateway(opt => {
  opt.Intents = GatewayIntents.GuildUsers;
})
.AddGatewayHandlers(typeof(Program).Assembly);


// builder.Services.AddHostedService<TTH>();
builder.Services.AddHttpClient<WikipediaClient>(e => {
  e.BaseAddress = new("https://api.wikimedia.org");
  e.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0"); // we lie <3
});


await builder.Build().RunAsync();
