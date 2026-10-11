using System.ComponentModel.DataAnnotations;

namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Represents a notification addressed to a single user. Notifications are
    /// persisted so they survive client disconnects and can be listed, counted
    /// and marked as read from the web and mobile clients.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Gets or sets the unique identifier for the notification.
        /// </summary>
        [Key]
        public int NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the recipient user (AspNetUsers.Id).
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the short title or subject of the notification.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the body text of the notification.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the category of the notification (e.g. Reminder, Booking, Chat, System).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets an optional client-side route or link related to the notification.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the recipient has read the notification.
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when the notification was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
