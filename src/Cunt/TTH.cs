using Microsoft.Extensions.Hosting;
using NetCord.Gateway;

namespace Cunt;

public class TTH(
  ILogger<TTH> logger,
  IConfiguration conf,
  WikipediaClient client,
  GatewayClient disc
) : BackgroundService {

  private readonly ILogger<TTH> _logger = logger;
  private readonly WikipediaClient _client = client;
  private readonly GatewayClient _discord = disc;
  private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(1));

  protected override async Task ExecuteAsync(CancellationToken ct) {
    var channelIdStr = conf["TTH:ChannelID"];
    if (!ulong.TryParse(channelIdStr, out var channelId)) {
      _logger.LogWarning("TTH:ChannelID is not configured or invalid. TTH background service will not post messages.");
      return;
    }

    do {
      try {
        var todaysEvent = await _client.GetTodaysEvent(ct);
        _logger.LogInformation("On this day in {year}: {event}", todaysEvent.Year, todaysEvent.Title);
        await _discord.SendMessageAsync(channelId, $"On this day in {todaysEvent.Year}: {todaysEvent.Title}");
      } catch (Exception ex) when (ex is not OperationCanceledException) {
        _logger.LogError(ex, "Failed to retrieve or send today's Wikipedia event.");
      }
    } while (await _timer.WaitForNextTickAsync(ct));
  }

  public override void Dispose() {
    _timer.Dispose();
    base.Dispose();
    GC.SuppressFinalize(this);
  }
}

