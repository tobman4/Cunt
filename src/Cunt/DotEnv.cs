namespace Cunt;

public static class DotEnv {
  public static void LoadEnvFile(string path) {
    if (!File.Exists(path)) {
      Console.WriteLine($"Found no env file: {path}");
      return;
    }

    var lines = File.ReadLines(path);

    foreach (var rawLine in lines) {
      var line = rawLine.Trim();

      if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) {
        continue;
      }

      var split = line.Split('=', 2);
      if (split.Length < 2) {
        Console.WriteLine($"Bad line in env file (missing '='): \"{rawLine}\"");
        continue;
      }

      var key = split[0].Trim();
      if (string.IsNullOrWhiteSpace(key)) {
        Console.WriteLine($"Bad line in env file (empty key): \"{rawLine}\"");
        continue;
      }

      var value = split[1].Trim();
      if (value.Length >= 2 &&
          ((value.StartsWith('"') && value.EndsWith('"')) ||
           (value.StartsWith('\'') && value.EndsWith('\'')))) {
        value = value[1..^1];
      }

      Environment.SetEnvironmentVariable(key, value);
    }
  }
}

