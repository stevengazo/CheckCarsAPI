
using System.ComponentModel.DataAnnotations;

namespace CheckCarsAPI.Models;
public class Commentary
{
    [Key]

    public int Id { get; set; }
    public string? Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Author { get; set; }
    public string? AuthorId { get; set; }
    public string? ReportId { get; set; }
    public Report? Report { get; set; }

}