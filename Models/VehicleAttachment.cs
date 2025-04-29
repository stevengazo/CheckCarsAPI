using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckCarsAPI.Models
{
    public class VehicleAttachment
    {
        [Key]
        public string AttachmentId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string? FileName { get; set; }

        [Required]
        public string? FilePath { get; set; }

        public string? Description { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Relación con el vehículo
        public int CarId { get; set; }

        [ForeignKey("CarId")]
        public Car? Car { get; set; }
    }
}
