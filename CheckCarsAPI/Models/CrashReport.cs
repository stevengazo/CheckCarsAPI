using System.ComponentModel.DataAnnotations;

namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Represents a report detailing information about a car crash incident.
    /// Inherits from the base Report class.
    /// </summary>
    public class CrashReport : Report
    {
        /// <summary>
        /// Gets or sets the date when the crash occurred.
        /// </summary>
        public DateTime DateOfCrash { get; set; }

        /// <summary>
        /// Gets or sets additional details describing the crash (optional).
        /// </summary>
        public string? CrashDetails { get; set; }

        /// <summary>
        /// Gets or sets the location where the crash happened (optional).
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Gets or sets the parts of the car that were damaged or crashed (optional).
        /// </summary>
        public string? CrashedParts { get; set; }
    }
}
