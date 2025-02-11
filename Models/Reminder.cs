

using System.ComponentModel.DataAnnotations;

namespace CheckCarsAPI.Models;

public class Reminder
{
    [Key]

    public int ReminderId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
      public string Email { get; set; }
    public DateTime ReminderDate { get; set; }
    public bool IsCompleted { get; set; }
    public string Author { get; set; }
    public string AuthorId { get; set; }
    public int CarId { get; set; }
    public Car Car { get; set; }
}