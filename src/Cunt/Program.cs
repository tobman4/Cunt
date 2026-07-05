global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using NetCord.Hosting.Gateway;
using NetCord.Gateway;

using Cunt;
using NetCord.Hosting.Services.ApplicationCommands;
using Cunt.Services;
using System.Text;
using NetCord.Services.ApplicationCommands;

DotEnv.LoadEnvFile("./.env");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<RoleEnforcer>();
builder.Services.AddScoped<WordGame>();

builder.Services.AddApplicationCommands();
builder.Services.AddDiscordGateway(opt => {
  opt.Intents = GatewayIntents.GuildUsers;
})
.AddGatewayHandlers(typeof(Program).Assembly);


// builder.Services.AddHostedService<TTH>();
builder.Services.AddHttpClient<WikipediaClient>(e => {
  e.BaseAddress = new("https://api.wikimedia.org");
  e.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0"); // we lie <3
});


var app = builder.Build();


app.AddSlashCommand("guess", "Guess the word of the day", (WordGame game, string guess) => {
  if(guess.Length > 25)
    return "Bad guess: To long";

  GuessResult[] result;
  try {
    result = game.HandleGuess(guess);
  } catch(ArgumentException err) {
    return $"Bad guess: {err.Message}";
  }

  var str = new StringBuilder();
  foreach(var r in result) {
    if(r == GuessResult.Correct) 
      str.Append(":green_square:");

    else if(r == GuessResult.WrongSpot)
      str.Append(":orange_square:");

    else
      str.Append(":red_square:");
  }


  return str.ToString();
});
await app.RunAsync();
