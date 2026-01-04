namespace Cunt;

static class DotEnv {
  public static void LoadEnvFile(string path) {
    if(!File.Exists(path)) {
      Console.WriteLine($"Found no env file: {path}");
    }

    var lines = File.ReadLines(path);

    foreach(var line in lines) {
      var split = line.Split("=");

      var key = split.FirstOrDefault();
      if(string.IsNullOrWhiteSpace(key)) {
        Console.WriteLine($"Bad line in env file: \"{line}\"");
        continue;
      }

      Environment.SetEnvironmentVariable(
        key,
        string.Join('=', split.Skip(1))
      );

    }

  }
}
