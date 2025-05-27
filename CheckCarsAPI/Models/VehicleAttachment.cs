using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Represents an attachment file related to a vehicle.
    /// </summary>
    public class VehicleAttachment
    {
        /// <summary>
        /// Gets or sets the unique identifier for the attachment.
        /// </summary>
        [Key]
        public string AttachmentId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the file name of the attachment.
        /// </summary>
        [Required]
        public string? FileName { get; set; }

        /// <summary>
        /// Gets or sets the file path or location of the attachment.
        /// </summary>
        [Required]
        public string? FilePath { get; set; }

        /// <summary>
        /// Gets or sets an optional description of the attachment.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the attachment was uploaded.
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the identifier of the car this attachment is related to.
        /// </summary>
        public string CarId { get; set; }

        /// <summary>
        /// Gets or sets the car associated with this attachment (optional).
        /// </summary>
        [ForeignKey("CarId")]
        public Car? Car { get; set; }
    }
}
