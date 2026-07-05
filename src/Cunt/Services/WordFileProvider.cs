using Cunt.Interfaces;

namespace Cunt.Services;

class WordFileProvider(IConfiguration conf, ILogger<WordFileProvider> logger) : IWordProvider {

  private readonly string _path = conf.GetValue<string>("WordFile", "./words");
  private readonly ILogger _logger = logger;

  public string[] GetWords() {
    if(!File.Exists(_path))
      throw new Exception("No word file");
    try {
      return File.ReadAllLines(_path);
    } catch(Exception err) {
      _logger.LogWarning(err, "Unable to get words");
      throw;
    }
  }
  

}
