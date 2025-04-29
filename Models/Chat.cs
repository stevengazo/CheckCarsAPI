using System.ComponentModel.DataAnnotations;

namespace CheckCarsAPI.Models;

public class Chat
{
    [Key]
    public int ChatId { get; set; }
    public string? Name { get; set; } // Para chats grupales o individuales (puede ser null)
    public ICollection<UserApp> Users { get; set; } = new List<UserApp>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
