using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckCarsAPI.Models;


// Message.cs
public class Message
{
    [Key]
    public int MessageId { get; set; }
    public int ChatId { get; set; }
   
    [ForeignKey("Sender")]
    public string SenderId { get; set; }

    public string Content { get; set; }
    public DateTime SentAt { get; set; }

    public Chat Chat { get; set; }
    public UserApp Sender { get; set; } 
}