using Microsoft.Extensions.Logging;

namespace Cunt.WordGame;

public class WordGame {
  private readonly object _lock = new();
  private readonly IWordProvider _wordProvider;
  private readonly ILogger<WordGame> _logger;
  private readonly Dictionary<ulong, GameContext> _activeGames = new();
  private string _secretWord = "Braum";

  public WordGame(IWordProvider wordProvider, ILogger<WordGame> logger) {
    _wordProvider = wordProvider;
    _logger = logger;

    try {
      var words = _wordProvider.GetWords();
      if (words.Length > 0) {
        _secretWord = words[Random.Shared.Next(words.Length)];
      }
    } catch (Exception ex) {
      _logger.LogWarning(ex, "Failed to load initial word from provider. Using default fallback '{word}'", _secretWord);
    }
  }

  public string SecretWord {
    get {
      lock (_lock) {
        return _secretWord;
      }
    }
  }

  public GameContext HandleGuess(ulong userID, string guess) {
    if (string.IsNullOrWhiteSpace(guess)) {
      throw new ArgumentException("Guess cannot be empty.");
    }

    guess = guess.Trim();

    lock (_lock) {
      if (guess.Length != _secretWord.Length) {
        throw new ArgumentException($"Guess must be {_secretWord.Length} letters.");
      }

      var results = new GuessResult[guess.Length];
      var secretMatched = new bool[_secretWord.Length];

      // First pass: Find exact matches (Correct)
      for (int i = 0; i < guess.Length; i++) {
        if (char.ToUpperInvariant(guess[i]) == char.ToUpperInvariant(_secretWord[i])) {
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

      if (!_activeGames.TryGetValue(userID, out var game)) {
        _logger.LogDebug("New game: {user}", userID);
        game = new GameContext(userID);
        _activeGames[userID] = game;
      }

      game.GuessLog.Add(results);

      if (results.All(r => r == GuessResult.Correct)) {
        _logger.LogDebug("Word found by {userID}", userID);
        _activeGames.Remove(userID);
      }

      return game;
    }
  }

  public string SelectNewWord() {
    var words = _wordProvider.GetWords();
    if (words.Length == 0) {
      throw new InvalidOperationException("No words available to select.");
    }

    var selectedWord = words[Random.Shared.Next(words.Length)];

    lock (_lock) {
      _secretWord = selectedWord;
      _activeGames.Clear();
      _logger.LogInformation("New word selected: {word}", _secretWord);
      return _secretWord;
    }
  }
}

