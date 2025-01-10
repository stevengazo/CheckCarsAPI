using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCarsAPI.Models
{
    public class Photo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string PhotoId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime DateTaken { get; set; }
        [ForeignKey("Report")]
        public string ReportId { get; set; }
        public Report Report { get; set; }

    }
}
