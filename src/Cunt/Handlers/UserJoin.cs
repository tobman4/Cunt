using NetCord;
using NetCord.Hosting.Gateway;

namespace Cunt.Handlers;

public class UserJoin(IConfiguration conf, ILogger<UserJoin> logger) : IGuildUserAddGatewayHandler {

  private readonly ILogger _logger = logger;
  private readonly ulong _roleID = ulong.Parse(conf["RoleID"] ?? "");
  private readonly ulong _serverID = ulong.Parse(conf["ServerID"] ?? "");

  public async ValueTask HandleAsync(GuildUser user) {
    if(user.GuildId != _serverID) {
      _logger.LogWarning("Event from unknown server {id}", user.GuildId);
      return;
    }

    _logger.LogInformation("New user {name}({id})", user.GlobalName, user.Id);

    try {
      await user.AddRoleAsync(_roleID);
    } catch(Exception err) {
      _logger.LogError(err, "Faild to set role on user {name}", user.GlobalName);
    }


  }
  
}
