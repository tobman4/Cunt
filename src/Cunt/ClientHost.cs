using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace Cunt;

class ClientHost(
		ILogger<ClientHost> log
) : BackgroundService {



	
	protected override Task ExecuteAsync(CancellationToken stoppingToken) {
		log.LogInformation("Start bot <3");
		return Task.CompletedTask;
	}

	public override async Task StopAsync(CancellationToken stoppingToken) {
		log.LogInformation("Stopping bot");
		
		await base.StopAsync(stoppingToken);
	}

}
