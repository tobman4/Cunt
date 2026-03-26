using System.Net.Http.Json;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace Cunt;

record Event(int Year, string Title);

class WikipediaClient(HttpClient client, ILogger<WikipediaClient> logger) {

  private readonly HttpClient _client = client;
  private readonly ILogger _logger = logger;
  private readonly Random _rng = new();

  public async Task<Event> GetTodaysEvent() {
    var today = DateTime.Now;
    var day = today.Day.ToString().PadLeft(2,'0');
    var month = today.Month.ToString().PadLeft(2,'0');


    var data = await _client.GetFromJsonAsync<JsonObject>($"feed/v1/wikipedia/en/onthisday/all/{month}/{day}");
    if(data is null)
      throw new Exception("Got bad data :(");

    var events = data["events"]!;
    var index = _rng.Next(events.AsArray().Count());

    var todaysEvent = events.AsArray()[index];

    var obj = new Event(
      todaysEvent?["year"]?.GetValue<int>() ?? throw new Exception("Cant get year"),
      todaysEvent?["text"]?.GetValue<string>() ?? throw new Exception("Cant get title")
    );

    if(string.IsNullOrWhiteSpace(obj.Title))
      _logger.LogWarning("God bad data date: {day}/{month} - index: {index}", day, month, index);
      
    return obj;

  }
}

