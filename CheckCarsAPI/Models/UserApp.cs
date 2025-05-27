using Microsoft.AspNetCore.Identity;

namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Represents an application user extending the IdentityUser class.
    /// </summary>
    public class UserApp : IdentityUser
    {
        /// <summary>
        /// Gets or sets the URL of the user's avatar image (optional).
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets the collection of chats the user is participating in.
        /// </summary>
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();

        /// <summary>
        /// Gets or sets the collection of messages sent by the user.
        /// </summary>
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
