using Microsoft.Extensions.Hosting;
using NetCord.Gateway;

namespace Cunt;

class TTH(
  ILogger<TTH> logger,
  IServiceProvider services,
  WikipediaClient client,
  GatewayClient disc
) : BackgroundService {

  private readonly ILogger _logger = logger;
  private readonly IServiceProvider _services = services;
  private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(60));
  private readonly WikipediaClient _client = client;
  private readonly GatewayClient _discord = disc;


  private DateTime _lastSend = DateTime.MinValue;
  private TimeSpan _timeToWait = TimeSpan.FromHours(1);

  protected override async Task ExecuteAsync(CancellationToken ct) {
     
    do{ 
      if(DateTime.UtcNow - _lastSend < _timeToWait) 
        continue;
      
      var todaysEvent = await _client.GetTodaysEvent();
      _logger.LogInformation("On this day in {year}: {event}", todaysEvent.Year, todaysEvent.Title);
      _lastSend = DateTime.UtcNow;
      
      await _discord.SendMessageAsync(0, 727421717252800562, $"On this day in {todaysEvent.Year}: {todaysEvent.Title}");
    }while(await _timer.WaitForNextTickAsync(ct));
  }
}
