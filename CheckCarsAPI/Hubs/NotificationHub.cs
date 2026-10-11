using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CheckCarsAPI.Hubs;

/// <summary>
/// Real-time channel for user notifications. Clients only need to connect
/// (authenticated); the server pushes notifications to each recipient through
/// <c>IHubContext.Clients.User(userId)</c>. SignalR resolves that user id from
/// the connection's NameIdentifier claim.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    // No client-callable methods: delivery is server-initiated and per-user,
    // which prevents a connected client from broadcasting to everyone.
}
