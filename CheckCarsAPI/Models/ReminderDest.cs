using System.ComponentModel.DataAnnotations;

namespace CheckCarsAPI.Models;

/// <summary>
/// Represents a destination or recipient for a reminder notification.
/// </summary>
public class ReminderDest
{
    /// <summary>
    /// Gets or sets the unique identifier for the reminder destination.
    /// </summary>
    [Key]
    public int ReminderDestId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier who will receive the reminder (optional).
    /// </summary>
    public string? UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the associated reminder.
    /// </summary>
    public int ReminderId { get; set; }

    /// <summary>
    /// Gets or sets the reminder related to this destination (optional).
    /// </summary>
    public Reminder? Reminder { get; set; }
}
