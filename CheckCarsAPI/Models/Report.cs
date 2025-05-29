using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Represents the base abstract class for different types of reports related to a car.
    /// </summary>
    public abstract class Report
    {
        /// <summary>
        /// Gets or sets the unique identifier for the report.
        /// </summary>
        [Key]
        public string ReportId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the author or creator of the report (optional).
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the report was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the license plate of the related car (optional).
        /// </summary>
        public string? CarPlate { get; set; }

        /// <summary>
        /// Gets or sets the latitude coordinate related to the report.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude coordinate related to the report.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets a numeric score or rating related to the report.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the report has been marked as deleted.
        /// </summary>
        public bool Deleted { get; set; } = false;

        /// <summary>
        /// Gets or sets the collection of photos attached to the report (optional).
        /// </summary>
        public ICollection<Photo>? Photos { get; set; } = new List<Photo>();

        /// <summary>
        /// Gets or sets the collection of commentaries associated with the report (optional).
        /// </summary>
        public ICollection<Commentary>? Commentaries { get; set; } = new List<Commentary>();

        /// <summary>
        /// Gets or sets the identifier of the car related to the report (optional).
        /// </summary>
        public string? CarId { get; set; }

        /// <summary>
        /// Gets or sets the car associated with the report (optional).
        /// </summary>
        public Car? Car { get; set; }
    }
}
