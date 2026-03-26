using Microsoft.Extensions.Hosting;
using NetCord.Gateway;
using NetCord;

namespace Cunt;

[Obsolete]
class RoleEnforcer(
	ILogger<RoleEnforcer> log,
	IConfiguration conf,
	GatewayClient client
) : BackgroundService {

	private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
	private readonly GatewayClient _client = client;


	private readonly List<GuildUser> _toAdd = new();

	protected override async Task ExecuteAsync(CancellationToken st) {
		log.LogInformation("Start bot <3");

		var serverID = ulong.Parse(conf["ServerID"] ?? "");
		var roleID = ulong.Parse(conf["RoleID"] ?? "");

		log.LogDebug("Server: {name}", serverID);
		log.LogDebug("Role: {name}", roleID);

		do {
			st.ThrowIfCancellationRequested();

			log.LogDebug("Roles to set {count}", _toAdd.Count());
			if(_toAdd.Count() > 0) {
				try {
					Task.WaitAll(new [] {
						SetRole(roleID),
						client.SetStatusOrder()
					});
				} catch(Exception err) {
					log.LogError(err, "Unable to set role");
				}
				continue;
			} else {
				await client.SetStatusInvestigating();
			}

			var g = await _client.Rest.GetGuildAsync(serverID);
			var all = g.GetUsersAsync();
			
			await foreach(var user in all) {
				if(!user.RoleIds.Contains(roleID)) {
					log.LogDebug("{name} missing role!!!", user.Username);
					_toAdd.Add(user);
				}
			}

			if(_toAdd.Count() == 0) 
				await _client.SetStatusSleep();

		} while(await _timer.WaitForNextTickAsync(st));
	}

	private async Task SetRole(ulong roleID) {
		var toDo = _toAdd.Take(5);

		log.LogDebug("Seting role on {count} users", toDo.Count());
		foreach(var user in toDo) {
			log.LogInformation("Setting role for {name}", user.Username);
			await user.AddRoleAsync(roleID);
			_toAdd.Remove(user);
		}

	}
}

