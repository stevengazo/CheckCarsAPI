
using System.ComponentModel.DataAnnotations;

namespace CheckCarsAPI.Models
{
    public class Car
    {
        [Key]
        public string CarId { get; set; } = Guid.NewGuid().ToString();
        public string Model { get; set; }
        public string? Type { get; set; }
        public string? FuelType  { get; set; }
        public string? Plate { get; set; }
        public string? ImagePath { get; set; }
        public DateTime? AdquisitionDate { get; set; }
        public string? VIN { get; set; }
        public string? Brand {  get; set; }
        public string? Color { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public double? Lenght { get; set; }
        public double? Weight { get; set; }
        public string? Notes { get; set; }
        
        public int? Year { get; set; }
        public ICollection<CarService>? Services { get; set; }
        public ICollection<EntryExitReport>? EntryExitReports { get; set; }
        public ICollection<IssueReport>? IssueReports { get; set; }
        public ICollection<CrashReport>? CrashReports { get; set; }
        public ICollection<VehicleReturn>? VehicleReturns { get; set; }
        public ICollection<Reminder>? Reminders { get; set; }
        public ICollection<VehicleAttachment>? VehicleAttachments { get; set; } 
        public ICollection<Booking>? Bookings { get; set; } = new List<Booking>();
    }
}
