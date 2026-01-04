using NetCord;
using NetCord.Gateway;


namespace Cunt;

static class DiscordExtensions {


	public static async Task SetStatusSleep(this GatewayClient client) {
		var act = new PresenceProperties(UserStatusType.Idle);
		act.Afk = true;
		act.Activities = new[]{ new UserActivityProperties("zzz ZZZ zzz ZZZ", UserActivityType.Playing) };

		await client.UpdatePresenceAsync(
			act
		);
			
	}

	public static async Task SetStatusInvestigating(this GatewayClient client) {
		var act = new PresenceProperties(UserStatusType.Online);
		act.Afk = false;
		act.Activities = new[]{ new UserActivityProperties("Investigating 🔍", UserActivityType.Playing) };

		await client.UpdatePresenceAsync(
			act
		);
			
	}

	public static async Task SetStatusOrder(this GatewayClient client) {
		var act = new PresenceProperties(UserStatusType.Online);
		act.Afk = false;
		act.Activities = new[]{ new UserActivityProperties("Restoring order 🧹", UserActivityType.Playing) };

		await client.UpdatePresenceAsync(
			act
		);
			
	}


}
