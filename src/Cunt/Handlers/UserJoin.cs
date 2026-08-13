using NetCord;
using NetCord.Hosting.Gateway;

namespace Cunt.Handlers;

public class UserJoin(IConfiguration conf, ILogger<UserJoin> logger) : IGuildUserAddGatewayHandler {

  private readonly ILogger _logger = logger;
  private readonly ulong _roleID = ulong.TryParse(conf["RoleID"], out var rId)
    ? rId
    : throw new InvalidOperationException("Configuration 'RoleID' is required and must be a valid ulong.");
  private readonly ulong _serverID = ulong.TryParse(conf["ServerID"], out var sId)
    ? sId
    : throw new InvalidOperationException("Configuration 'ServerID' is required and must be a valid ulong.");

  public async ValueTask HandleAsync(GuildUser user) {
    if (user.GuildId != _serverID) {
      _logger.LogWarning("Event from unknown server {id}", user.GuildId);
      return;
    }

    var displayName = user.GlobalName ?? user.Username;
    _logger.LogInformation("New user {name} ({id})", displayName, user.Id);

    try {
      await user.AddRoleAsync(_roleID);
    } catch (Exception err) {
      _logger.LogError(err, "Failed to set role on user {name}", displayName);
    }
  }
}

