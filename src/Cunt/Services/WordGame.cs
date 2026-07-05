using Microsoft.Extensions.DependencyInjection;
using Cunt.Interfaces;

namespace Cunt.Services;

enum GuessResult {
  Wrong,
  Correct,
  WrongSpot
}

class GameContext(string user) {
  public readonly string UserID = user;

  public List<GuessResult[]> _guessLog = new();
}

class WordGame(IServiceProvider services, ILogger<WordGame> logger) {

  private static readonly Random _rng = new();
  private static string _secretWord = "Braum";

  private readonly IServiceProvider _services = services;
  private readonly ILogger _logger = logger;


  public GuessResult[] HandleGuess(string guess) {
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

    return results;
  }

  public void SelectNewWord() {

    using var scope = _services.CreateAsyncScope();
    var provider = scope.ServiceProvider.GetServices<IWordProvider>()
      .First();

    var words = provider.GetWords();
    _rng.Shuffle(words);
    
    _secretWord = words.First();
    _logger.LogInformation("New word: {word}", _secretWord);
  }

}
