using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Services.ApplicationCommands;
using System.Text;

namespace Cunt.WordGame;

public static class WordGameExtensions {
  public static IServiceCollection AddWordGame(this IServiceCollection services) {
    services.AddScoped<WordGame>();
    services.AddScoped<IWordProvider, WordFileProvider>();
    return services;
  }

  public static async Task<IHost> MapWordGameCommandsAsync(this IHost host) {
    var featureManager = host.Services.GetRequiredService<IFeatureManager>();
    
    if (await featureManager.IsEnabledAsync(Cunt.FeatureFlags.WordGameEnabled)) {
      host.AddSlashCommand("guess", "Guess the word of the day", (WordGame game, ApplicationCommandContext ctx, string guess) => {
        if (guess.Length > 25)
          return "Bad guess: To long";

        GameContext result;
        try {
          result = game.HandleGuess(ctx.User.Id, guess);
        } catch (ArgumentException err) {
          return $"Bad guess: {err.Message}";
        }

        var str = new StringBuilder();
        foreach (var round in result.GuessLog) {
          foreach (var r in round) {
            if (r == GuessResult.Correct) 
              str.Append(":green_square:");
            else if (r == GuessResult.WrongSpot)
              str.Append(":orange_square:");
            else
              str.Append(":red_square:");
          }
          str.Append("\n");
        }

        return str.ToString();
      });

      host.AddSlashCommand("rotate", "Set new word of the day", (WordGame game, ApplicationCommandContext ctx) => {
        game.SelectNewWord();
        return "New word :o";
      });
    }

    return host;
  }
}
