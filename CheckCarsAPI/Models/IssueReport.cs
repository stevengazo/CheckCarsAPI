using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Represents a report describing an issue related to a vehicle.
    /// Inherits from the base Report class.
    /// </summary>
    public class IssueReport : Report
    {
        /// <summary>
        /// Gets or sets detailed information about the issue (optional).
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Gets or sets the priority level of the issue (optional).
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Gets or sets the type or category of the issue (optional).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the issue has been resolved.
        /// </summary>
        public bool IsResolved { get; set; } = false;
    }
}
