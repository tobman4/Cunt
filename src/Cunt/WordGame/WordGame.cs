using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cunt.WordGame;

public class WordGame(IServiceProvider services, ILogger<WordGame> logger) {

  private static readonly Random _rng = new();
  private static string _secretWord = "Braum";
  private static readonly Dictionary<ulong,GameContext> _activeGames = new();

  private readonly IServiceProvider _services = services;
  private readonly ILogger _logger = logger;

  private GameContext GetOrCreateContext(ulong userID) {
    if(_activeGames.ContainsKey(userID))
      return _activeGames[userID];

    _logger.LogDebug("New game: {user}", userID);
    var ctx = new GameContext(userID);
    _activeGames.Add(userID, ctx);

    return ctx;
  }

  public GameContext HandleGuess(ulong userID, string guess) {
    if (string.IsNullOrWhiteSpace(guess))
      throw new ArgumentException("Guess cant be empty");
  
    var results = new GuessResult[guess.Length];
    var secretMatched = new bool[_secretWord.Length];

    // First pass: Find exact matches (Correct)
    for (int i = 0; i < guess.Length; i++) {
      if (i < _secretWord.Length && char.ToUpperInvariant(guess[i]) == char.ToUpperInvariant(_secretWord[i])) {
        results[i] = GuessResult.Correct;
        secretMatched[i] = true;
      }
    }

    // Second pass: Find wrong spot matches (WrongSpot)
    for (int i = 0; i < guess.Length; i++) {
      if (results[i] == GuessResult.Correct) {
        continue;
      }

      bool found = false;
      for (int j = 0; j < _secretWord.Length; j++) {
        if (!secretMatched[j] && char.ToUpperInvariant(guess[i]) == char.ToUpperInvariant(_secretWord[j])) {
          results[i] = GuessResult.WrongSpot;
          secretMatched[j] = true;
          found = true;
          break;
        }
      }

      if (!found) {
        results[i] = GuessResult.Wrong;
      }
    }

    var game = GetOrCreateContext(userID);
    game.GuessLog.Add(results);

    if(results.Count(e => e != GuessResult.Correct) == 0) {
      _logger.LogDebug("Word found by {userID}", userID);
      _activeGames.Remove(userID);
    }

    return game;
  }

  public void SelectNewWord() {

    using var scope = _services.CreateAsyncScope();
    var provider = scope.ServiceProvider.GetServices<IWordProvider>()
      .First();

    var words = provider.GetWords();
    _rng.Shuffle(words);
    
    _secretWord = words.First();
    _logger.LogInformation("New word: {word}", _secretWord);
    _activeGames.Clear();
  }

}
