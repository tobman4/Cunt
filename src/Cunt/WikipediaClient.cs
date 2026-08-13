using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace Cunt;

public record Event(int Year, string Title);

public class WikipediaClient(HttpClient client, ILogger<WikipediaClient> logger) {
  private readonly HttpClient _client = client;
  private readonly ILogger<WikipediaClient> _logger = logger;

  public async Task<Event> GetTodaysEvent(CancellationToken cancellationToken = default) {
    var today = DateTime.UtcNow;
    var day = today.Day.ToString().PadLeft(2, '0');
    var month = today.Month.ToString().PadLeft(2, '0');

    var data = await _client.GetFromJsonAsync<JsonObject>($"feed/v1/wikipedia/en/onthisday/all/{month}/{day}", cancellationToken);
    if (data is null) {
      throw new InvalidOperationException("Wikipedia API returned empty response.");
    }

    var events = data["events"]?.AsArray();
    if (events is null || events.Count == 0) {
      throw new InvalidOperationException($"No events found for {month}/{day}.");
    }

    var index = Random.Shared.Next(events.Count);
    var todaysEvent = events[index];

    var year = todaysEvent?["year"]?.GetValue<int>()
      ?? throw new InvalidOperationException("Unable to parse event year.");
    var title = todaysEvent?["text"]?.GetValue<string>()
      ?? throw new InvalidOperationException("Unable to parse event title.");

    if (string.IsNullOrWhiteSpace(title)) {
      _logger.LogWarning("Got empty event title for date: {day}/{month} - index: {index}", day, month, index);
    }

    return new Event(year, title);
  }
}


