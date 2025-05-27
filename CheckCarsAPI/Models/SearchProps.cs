namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Represents the properties used to perform a search query.
    /// </summary>
    public class SearchProps
    {
        /// <summary>
        /// Gets or sets the date to filter the search results.
        /// </summary>
        public DateTime date { get; set; }

        /// <summary>
        /// Gets or sets the vehicle's license plate to filter the search results (optional).
        /// </summary>
        public string? plate { get; set; }

        /// <summary>
        /// Gets or sets the author or creator name to filter the search results (optional).
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets the card identifier used in the search filter.
        /// </summary>
        public int CardId { get; set; }
    }
}
