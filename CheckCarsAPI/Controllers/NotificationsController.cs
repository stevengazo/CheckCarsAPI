using System.Security.Claims;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using CheckCarsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public NotificationsController(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>
        /// Returns the current user's notifications, newest first.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetMyNotifications(
            [FromQuery] bool unreadOnly = false,
            [FromQuery] int take = 50,
            [FromQuery] int skip = 0)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            take = Math.Clamp(take, 1, 200);
            skip = Math.Max(skip, 0);

            var query = _context.Notifications.Where(n => n.UserId == userId);
            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// Returns the number of unread notifications for the current user.
        /// </summary>
        [HttpGet("unread/count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Ok(new { count });
        }

        /// <summary>
        /// Marks a single notification as read. Only the owner can mark it.
        /// </summary>
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

            if (notification == null)
            {
                return NotFound();
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        /// <summary>
        /// Marks every unread notification of the current user as read.
        /// </summary>
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

            return NoContent();
        }

        /// <summary>
        /// Deletes one of the current user's notifications.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

            if (notification == null)
            {
                return NotFound();
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Sends a notification to a specific user or broadcasts it to everyone.
        /// Restricted to administrators and managers.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Send([FromBody] SendNotificationModel model)
        {
            if (model == null || (string.IsNullOrWhiteSpace(model.Title) && string.IsNullOrWhiteSpace(model.Message)))
            {
                return BadRequest("Title or Message is required.");
            }

            if (model.Broadcast)
            {
                await _notificationService.BroadcastAsync(model.Title, model.Message, model.Type, model.Url);
                return Ok(new { result = "Broadcast sent." });
            }

            if (string.IsNullOrWhiteSpace(model.UserId))
            {
                return BadRequest("UserId is required when Broadcast is false.");
            }

            var notification = await _notificationService.NotifyUserAsync(
                model.UserId, model.Title, model.Message, model.Type, model.Url);

            return Ok(notification);
        }

        public class SendNotificationModel
        {
            public string? UserId { get; set; }
            public bool Broadcast { get; set; }
            public string? Title { get; set; }
            public string? Message { get; set; }
            public string? Type { get; set; }
            public string? Url { get; set; }
        }
    }
}
