using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Reporte capturado a partir de una definición de formulario dinámica.
    /// Los valores de los campos se guardan como JSON en <see cref="DataJson"/>.
    /// </summary>
    public class DynamicReport
    {
        [Key]
        public string ReportId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Tipo de reporte: "Exit" o "Entry".</summary>
        [Required]
        public string Type { get; set; }

        /// <summary>Definición de formulario usada en la captura.</summary>
        public string? FormDefinitionId { get; set; }

        // Campos núcleo comunes (relacionales, consultables).
        public string? Author { get; set; }
        public DateTime Created { get; set; }
        public string? CarPlate { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Score { get; set; }
        public bool Deleted { get; set; } = false;

        public string? CarId { get; set; }
        public Car? Car { get; set; }

        /// <summary>Valores de los campos dinámicos serializados como JSON ({ key: value }).</summary>
        public string DataJson { get; set; } = "{}";

        /// <summary>Fotos asociadas (etiquetadas por slot vía Photo.SlotKey).</summary>
        public ICollection<Photo>? Photos { get; set; } = new List<Photo>();
    }
}
