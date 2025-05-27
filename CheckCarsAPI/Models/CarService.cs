namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Represents a service record performed on a car.
    /// </summary>
    public class CarService
    {
        /// <summary>
        /// Gets or sets the unique identifier for the car service record.
        /// </summary>
        public int CarServiceId { get; set; }

        /// <summary>
        /// Gets or sets the title or name of the service.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the date when the service was performed.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the type or category of the service (optional).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets a detailed description of the service (optional).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the mileage of the car at the time of the service.
        /// </summary>
        public int mileage { get; set; }

        /// <summary>
        /// Gets or sets the mileage at which the next service is due.
        /// </summary>
        public int NextMileage { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the car associated with this service.
        /// </summary>
        public string CarId { get; set; }

        /// <summary>
        /// Gets or sets the car related to this service record.
        /// </summary>
        public Car? Car { get; set; }
    }
}
