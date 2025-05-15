using System.ComponentModel.DataAnnotations;

namespace CheckCarsAPI.Models;

public class ReminderDest
{
    [Key]   
    public int ReminderDestId { get; set; }
    public string? UserId { get; set; } = string.Empty;
    public int ReminderId { get; set; }
    public Reminder? Reminder { get; set; }
}