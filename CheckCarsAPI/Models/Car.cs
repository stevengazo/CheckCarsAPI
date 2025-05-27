using System.ComponentModel.DataAnnotations;

namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Represents a car entity with detailed information.
    /// </summary>
    public class Car
    {
        /// <summary>
        /// Gets or sets the unique identifier for the car.
        /// </summary>
        [Key]
        public string CarId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the model name of the car.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the type of the car (optional).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the fuel type used by the car (optional).
        /// </summary>
        public string? FuelType { get; set; }

        /// <summary>
        /// Gets or sets the license plate of the car (optional).
        /// </summary>
        public string? Plate { get; set; }

        /// <summary>
        /// Gets or sets the image path or URL for the car's picture (optional).
        /// </summary>
        public string? ImagePath { get; set; }

        /// <summary>
        /// Gets or sets the acquisition date of the car (optional).
        /// </summary>
        public DateTime? AdquisitionDate { get; set; }

        /// <summary>
        /// Gets or sets the Vehicle Identification Number (VIN) of the car (optional).
        /// </summary>
        public string? VIN { get; set; }

        /// <summary>
        /// Gets or sets the brand or manufacturer of the car (optional).
        /// </summary>
        public string? Brand { get; set; }

        /// <summary>
        /// Gets or sets the color of the car (optional).
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets the width of the car in appropriate units (optional).
        /// </summary>
        public double? Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the car in appropriate units (optional).
        /// </summary>
        public double? Height { get; set; }

        /// <summary>
        /// Gets or sets the length of the car in appropriate units (optional).
        /// </summary>
        public double? Lenght { get; set; }

        /// <summary>
        /// Gets or sets the weight of the car in appropriate units (optional).
        /// </summary>
        public double? Weight { get; set; }

        /// <summary>
        /// Gets or sets additional notes about the car (optional).
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the car has been marked as deleted.
        /// </summary>
        public bool Deleted { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the car is currently available.
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Gets or sets the manufacturing year of the car (optional).
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Gets or sets the collection of service records associated with the car.
        /// </summary>
        public ICollection<CarService>? Services { get; set; }

        /// <summary>
        /// Gets or sets the collection of entry and exit reports related to the car.
        /// </summary>
        public ICollection<EntryExitReport>? EntryExitReports { get; set; }

        /// <summary>
        /// Gets or sets the collection of issue reports associated with the car.
        /// </summary>
        public ICollection<IssueReport>? IssueReports { get; set; }

        /// <summary>
        /// Gets or sets the collection of crash reports related to the car.
        /// </summary>
        public ICollection<CrashReport>? CrashReports { get; set; }

        /// <summary>
        /// Gets or sets the collection of vehicle return records for the car.
        /// </summary>
        public ICollection<VehicleReturn>? VehicleReturns { get; set; }

        /// <summary>
        /// Gets or sets the collection of reminders associated with the car.
        /// </summary>
        public ICollection<Reminder>? Reminders { get; set; }

        /// <summary>
        /// Gets or sets the collection of attachments related to the car.
        /// </summary>
        public ICollection<VehicleAttachment>? VehicleAttachments { get; set; }

        /// <summary>
        /// Gets or sets the collection of bookings made for the car.
        /// </summary>
        public ICollection<Booking>? Bookings { get; set; } = new List<Booking>();
    }
}
