namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Represents a report for the return of a vehicle, including mileage and notes.
    /// Inherits from the base Report class.
    /// </summary>
    public class VehicleReturn : Report
    {
        /// <summary>
        /// Gets or sets the mileage recorded when the vehicle was returned.
        /// </summary>
        public long mileage { get; set; }

        /// <summary>
        /// Gets or sets notes related to the vehicle return, e.g., oil level (optional).
        /// </summary>
        public string? Notes { get; set; }
    }
}
