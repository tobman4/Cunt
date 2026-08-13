using Microsoft.Extensions.Hosting;
using NetCord.Gateway;
using NetCord;

namespace Cunt;

public class RoleEnforcer(
  ILogger<RoleEnforcer> log,
  IConfiguration conf,
  GatewayClient client
) : BackgroundService {

  private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(1));
  private readonly GatewayClient _client = client;
  private readonly List<GuildUser> _toAdd = new();
  private readonly HashSet<ulong> _queuedUserIds = new();

  protected override async Task ExecuteAsync(CancellationToken st) {
    log.LogInformation("RoleEnforcer starting");

    var serverIDStr = conf["ServerID"] ?? throw new InvalidOperationException("Configuration 'ServerID' is required.");
    if (!ulong.TryParse(serverIDStr, out var serverID)) {
      throw new InvalidOperationException($"Configuration 'ServerID' ('{serverIDStr}') must be a valid ulong.");
    }

    var roleIDStr = conf["RoleID"] ?? throw new InvalidOperationException("Configuration 'RoleID' is required.");
    if (!ulong.TryParse(roleIDStr, out var roleID)) {
      throw new InvalidOperationException($"Configuration 'RoleID' ('{roleIDStr}') must be a valid ulong.");
    }

    log.LogDebug("Server ID: {serverID}, Role ID: {roleID}", serverID, roleID);

    do {
      st.ThrowIfCancellationRequested();

      log.LogDebug("Roles to set: {count}", _toAdd.Count);
      if (_toAdd.Count > 0) {
        try {
          await SetRoleAsync(roleID, st);
        } catch (Exception err) when (err is not OperationCanceledException) {
          log.LogError(err, "Unable to set role batch");
        }

        if (_toAdd.Count > 0) {
          await Task.Delay(1000, st);
          continue;
        }
      }

      try {
        var g = await _client.Rest.GetGuildAsync(serverID, cancellationToken: st);
        var all = g.GetUsersAsync();

        await foreach (var user in all.WithCancellation(st)) {
          if (!user.RoleIds.Contains(roleID) && _queuedUserIds.Add(user.Id)) {
            log.LogDebug("{name} missing role!", user.Username);
            _toAdd.Add(user);
          }
        }
      } catch (Exception ex) when (ex is not OperationCanceledException) {
        log.LogError(ex, "Failed to fetch guild users for server {serverID}", serverID);
      }

    } while (await _timer.WaitForNextTickAsync(st));
  }

  private async Task SetRoleAsync(ulong roleID, CancellationToken ct) {
    var toDo = _toAdd.Take(5).ToList();

    log.LogDebug("Setting role on {count} users", toDo.Count);
    foreach (var user in toDo) {
      ct.ThrowIfCancellationRequested();
      try {
        log.LogInformation("Setting role for {name}", user.Username);
        await user.AddRoleAsync(roleID);
      } catch (Exception err) {
        log.LogError(err, "Failed to set role for {name} ({id})", user.Username, user.Id);
      } finally {
        _toAdd.Remove(user);
        _queuedUserIds.Remove(user.Id);
      }
    }
  }

  public override void Dispose() {
    _timer.Dispose();
    base.Dispose();
    GC.SuppressFinalize(this);
  }
}


