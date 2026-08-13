using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cunt.WordGame;

public class WordFileProvider(IConfiguration conf, ILogger<WordFileProvider> logger) : IWordProvider {

  private readonly string _path = conf.GetValue<string>("WordFile", "./words");
  private readonly ILogger _logger = logger;

  public string[] GetWords() {
    if (!File.Exists(_path)) {
      _logger.LogError("Word file not found at path: {path}", _path);
      throw new FileNotFoundException($"Word file not found at '{_path}'.", _path);
    }

    try {
      var words = File.ReadLines(_path)
        .Where(w => !string.IsNullOrWhiteSpace(w))
        .Select(w => w.Trim())
        .ToArray();

      if (words.Length == 0) {
        throw new InvalidOperationException($"Word file at '{_path}' contains no valid words.");
      }

      return words;
    } catch (Exception err) when (err is not FileNotFoundException && err is not InvalidOperationException) {
      _logger.LogError(err, "Unable to read words from file: {path}", _path);
      throw;
    }
  }
}

