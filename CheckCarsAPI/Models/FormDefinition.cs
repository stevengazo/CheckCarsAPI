using System.ComponentModel.DataAnnotations;

namespace CheckCarsAPI.Models
{
    /// <summary>
    /// Definición configurable de un formulario de reporte (Salida/Entrada).
    /// Los campos y slots de foto se almacenan como columnas JSON.
    /// </summary>
    public class FormDefinition
    {
        [Key]
        public string FormDefinitionId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Tipo de formulario: "Exit" (salida) o "Entry" (entrada/entrega).</summary>
        [Required]
        public string Type { get; set; }

        public string? Name { get; set; }

        /// <summary>Versión incremental de la definición.</summary>
        public int Version { get; set; } = 1;

        /// <summary>Indica si es la definición activa para su tipo.</summary>
        public bool IsActive { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Campos del formulario (columna JSON).</summary>
        public List<FormField> Fields { get; set; } = new();

        /// <summary>Slots de foto requeridos/opcionales (columna JSON).</summary>
        public List<PhotoSlot> PhotoSlots { get; set; } = new();
    }

    /// <summary>Definición de un campo dinámico del formulario.</summary>
    public class FormField
    {
        public string Key { get; set; }
        public string Label { get; set; }

        /// <summary>text | number | select | checkbox | range | date | textarea</summary>
        public string Type { get; set; } = "text";

        public bool Required { get; set; }
        public int Order { get; set; }

        /// <summary>Opciones para type == "select".</summary>
        public List<string>? Options { get; set; }

        /// <summary>Límites para type number/range.</summary>
        public double? Min { get; set; }
        public double? Max { get; set; }
    }

    /// <summary>Definición de un slot de foto del formulario.</summary>
    public class PhotoSlot
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public bool Required { get; set; }

        /// <summary>Permite múltiples imágenes para el mismo slot.</summary>
        public bool Multiple { get; set; }
        public int Order { get; set; }
    }
}
