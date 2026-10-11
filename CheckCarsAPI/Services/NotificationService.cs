using CheckCarsAPI.Data;
using CheckCarsAPI.Hubs;
using CheckCarsAPI.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CheckCarsAPI.Services
{
    /// <summary>
    /// Creates and delivers user notifications. Every notification is persisted
    /// (so it can be listed/counted/marked read later) and, when the recipient is
    /// connected, pushed in real time through <see cref="NotificationHub"/> using
    /// per-user targeting (<c>Clients.User</c>), which maps to the user's
    /// NameIdentifier claim.
    /// </summary>
    public class NotificationService
    {
        /// <summary>Name of the SignalR client method invoked on the recipient.</summary>
        public const string ClientEvent = "ReceiveNotification";

        private readonly ApplicationDbContext _db;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(ApplicationDbContext db, IHubContext<NotificationHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        /// <summary>
        /// Persists a notification for a single user and pushes it in real time.
        /// </summary>
        public async Task<Notification> NotifyUserAsync(
            string userId,
            string? title,
            string? message,
            string? type = null,
            string? url = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                Url = url,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            await PushAsync(userId, notification);
            return notification;
        }

        /// <summary>
        /// Persists and pushes the same notification to several users.
        /// </summary>
        public async Task NotifyUsersAsync(
            IEnumerable<string> userIds,
            string? title,
            string? message,
            string? type = null,
            string? url = null)
        {
            var distinctIds = userIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (distinctIds.Count == 0)
            {
                return;
            }

            var created = new List<Notification>();
            foreach (var userId in distinctIds)
            {
                created.Add(new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Url = url,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _db.Notifications.AddRange(created);
            await _db.SaveChangesAsync();

            foreach (var notification in created)
            {
                await PushAsync(notification.UserId, notification);
            }
        }

        /// <summary>
        /// Persists a notification for every user and pushes it to all connected clients.
        /// Intended for administrative broadcasts.
        /// </summary>
        public async Task BroadcastAsync(
            string? title,
            string? message,
            string? type = null,
            string? url = null)
        {
            var userIds = await _db.Users.Select(u => u.Id).ToListAsync();
            await NotifyUsersAsync(userIds, title, message, type, url);
        }

        private Task PushAsync(string userId, Notification notification)
        {
            return _hub.Clients.User(userId).SendAsync(ClientEvent, new
            {
                notification.NotificationId,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.Url,
                notification.IsRead,
                notification.CreatedAt
            });
        }
    }
}
