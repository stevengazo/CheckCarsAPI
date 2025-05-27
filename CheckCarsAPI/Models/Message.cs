using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Represents a message sent within a chat.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the unique identifier for the message.
        /// </summary>
        [Key]
        public int MessageId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the chat this message belongs to.
        /// </summary>
        public int ChatId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who sent the message (optional).
        /// </summary>
        [ForeignKey("Sender")]
        public string? SenderId { get; set; }

        /// <summary>
        /// Gets or sets the content or text of the message (optional).
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the message was sent.
        /// </summary>
        public DateTime SentAt { get; set; }

        /// <summary>
        /// Gets or sets the chat associated with this message (optional).
        /// </summary>
        public Chat? Chat { get; set; }

        /// <summary>
        /// Gets or sets the user who sent the message (optional).
        /// </summary>
        public UserApp? Sender { get; set; }
    }
}
