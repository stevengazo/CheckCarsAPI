using System.ComponentModel.DataAnnotations;

namespace CheckCarsAPI.Models;

/// <summary>
/// Represents a reminder associated with a car and its related notifications.
/// </summary>
public class Reminder
{
    /// <summary>
    /// Gets or sets the unique identifier for the reminder.
    /// </summary>
    [Key]
    public int ReminderId { get; set; }

    /// <summary>
    /// Gets or sets the title or subject of the reminder (optional).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets a detailed description of the reminder (optional).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the email address related to the reminder (optional).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the reminder is set to occur.
    /// </summary>
    public DateTime ReminderDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the reminder has been completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the reminder should be sent.
    /// </summary>
    public bool SendIt { get; set; }

    /// <summary>
    /// Gets or sets the author or creator of the reminder (optional).
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the car associated with the reminder.
    /// </summary>
    public string CarId { get; set; }

    /// <summary>
    /// Gets or sets the car related to this reminder (optional).
    /// </summary>
    public Car? Car { get; set; }

    /// <summary>
    /// Gets or sets the list of reminder destinations or recipients (optional).
    /// </summary>
    public List<ReminderDest>? ReminderDests { get; set; } = new List<ReminderDest>();
}
