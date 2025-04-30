using Microsoft.AspNetCore.Identity;

namespace CheckCarsAPI.Models
{
    public class UserApp : IdentityUser
    {
         public string? AvatarUrl { get; set; }

        // Relaciones
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}