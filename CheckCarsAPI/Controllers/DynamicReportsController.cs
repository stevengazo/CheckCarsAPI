using System.Text.Json;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using CheckCarsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,User,Guest")]
    public class DynamicReportsController : ControllerBase
    {
        private readonly ReportsDbContext _context;
        private readonly IFileService _fileService;

        public DynamicReportsController(ReportsDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: api/DynamicReports/search?type=Exit&plate=&author=&date=
        [HttpGet("search")]
        public async Task<IActionResult> Search(string? type = null, string? plate = null, string? author = null, DateTime? date = null)
        {
            var q = _context.DynamicReports.Include(d => d.Photos).Where(d => !d.Deleted);
            if (!string.IsNullOrWhiteSpace(type)) q = q.Where(d => d.Type == type);
            if (!string.IsNullOrWhiteSpace(plate)) q = q.Where(d => d.CarPlate == plate);
            if (!string.IsNullOrWhiteSpace(author)) q = q.Where(d => d.Author == author);
            if (date.HasValue) q = q.Where(d => d.Created.Date == date.Value.Date);
            return Ok(await q.OrderByDescending(d => d.Created).ToListAsync());
        }

        [HttpGet("ByCar/{carId}")]
        public async Task<IActionResult> ByCar(string carId, string? type = null)
        {
            var q = _context.DynamicReports.Include(d => d.Photos).Where(d => d.CarId == carId && !d.Deleted);
            if (!string.IsNullOrWhiteSpace(type)) q = q.Where(d => d.Type == type);
            return Ok(await q.OrderByDescending(d => d.Created).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var r = await _context.DynamicReports.Include(d => d.Photos).FirstOrDefaultAsync(d => d.ReportId == id);
            return r == null ? NotFound() : Ok(r);
        }

        /// <summary>Crea un reporte dinámico. Multipart: campo "data" (JSON) + archivos por slotKey.</summary>
        [HttpPost("form")]
        public async Task<IActionResult> Create([FromForm] string type, [FromForm] string data, [FromForm] string? carId = null)
        {
            if (string.IsNullOrWhiteSpace(type))
                return BadRequest("El tipo es obligatorio.");

            // Definición activa para validar.
            var def = await _context.FormDefinitions
                .Include(f => f.Fields).Include(f => f.PhotoSlots)
                .Where(f => f.Type == type && f.IsActive)
                .OrderByDescending(f => f.Version)
                .FirstOrDefaultAsync();
            if (def == null)
                return BadRequest($"No hay una definición activa para el tipo '{type}'.");

            Dictionary<string, JsonElement> values;
            try
            {
                values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(data ?? "{}") ?? new();
            }
            catch
            {
                return BadRequest("El campo 'data' no es JSON válido.");
            }

            // Validación de campos obligatorios.
            var missingFields = def.Fields
                .Where(f => f.Required)
                .Where(f => !values.TryGetValue(f.Key, out var v) || IsEmpty(v))
                .Select(f => f.Label ?? f.Key)
                .ToList();

            // Validación de slots de foto obligatorios.
            var files = Request.Form.Files;
            var missingPhotos = def.PhotoSlots
                .Where(s => s.Required)
                .Where(s => !files.Any(file => file.Name == s.Key))
                .Select(s => s.Label ?? s.Key)
                .ToList();

            if (missingFields.Count > 0 || missingPhotos.Count > 0)
            {
                return BadRequest(new
                {
                    message = "Faltan campos o fotos obligatorios.",
                    missingFields,
                    missingPhotos
                });
            }

            var report = new DynamicReport
            {
                ReportId = Guid.NewGuid().ToString(),
                Type = type,
                FormDefinitionId = def.FormDefinitionId,
                CarId = carId,
                Author = User?.Identity?.Name,
                Created = DateTime.UtcNow,
                DataJson = data ?? "{}",
                Photos = new List<Photo>()
            };

            if (!string.IsNullOrWhiteSpace(carId))
            {
                var car = await _context.Cars.FirstOrDefaultAsync(c => c.CarId == carId);
                report.CarPlate = car?.Plate;
            }

            // Guarda las fotos etiquetadas por slot (nombre de campo = slotKey).
            foreach (var file in files)
            {
                if (file.Length == 0) continue;
                var path = (await _fileService.SaveFileAsync(file, "reports")).Replace("\\", "/");
                report.Photos.Add(new Photo
                {
                    PhotoId = Guid.NewGuid().ToString(),
                    FileName = file.FileName,
                    FilePath = path,
                    DateTaken = DateTime.UtcNow,
                    SlotKey = file.Name,
                    DynamicReportId = report.ReportId
                });
            }

            _context.DynamicReports.Add(report);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = report.ReportId }, report);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(string id)
        {
            var r = await _context.DynamicReports.FirstOrDefaultAsync(d => d.ReportId == id);
            if (r == null) return NotFound();
            r.Deleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Migra reportes fijos (EntryExitReport/VehicleReturn) al modelo dinámico. Idempotente.</summary>
        [HttpPost("migrate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Migrate()
        {
            var existing = await _context.DynamicReports.Select(d => d.ReportId).ToHashSetAsync();
            int exits = 0, entries = 0;

            foreach (var e in await _context.EntryExitReports.ToListAsync())
            {
                if (existing.Contains(e.ReportId)) continue;
                var data = new Dictionary<string, object?>
                {
                    ["mileage"] = e.mileage,
                    ["fuelLevel"] = e.FuelLevel,
                    ["oilLevel"] = e.OilLevel,
                    ["tiresState"] = e.TiresState,
                    ["paintState"] = e.PaintState,
                    ["mecanicState"] = e.MecanicState,
                    ["interiorsState"] = e.InteriorsState,
                    ["hasSpareTire"] = e.HasSpareTire,
                    ["hasChargerUSB"] = e.HasChargerUSB,
                    ["hasQuickPass"] = e.HasQuickPass,
                    ["hasPhoneSupport"] = e.HasPhoneSupport,
                    ["hasEmergencyKit"] = e.HasEmergencyKit,
                    ["notes"] = e.Notes,
                };
                _context.DynamicReports.Add(MapCore(e.ReportId, "Exit", e.CarId, e.Author, e.Created, e.CarPlate, e.Latitude, e.Longitude, e.Score, data));
                exits++;
            }

            foreach (var r in await _context.VehicleReturns.ToListAsync())
            {
                if (existing.Contains(r.ReportId)) continue;
                var data = new Dictionary<string, object?> { ["mileage"] = r.mileage, ["notes"] = r.Notes };
                _context.DynamicReports.Add(MapCore(r.ReportId, "Entry", r.CarId, r.Author, r.Created, r.CarPlate, r.Latitude, r.Longitude, r.Score, data));
                entries++;
            }

            await _context.SaveChangesAsync();
            return Ok(new { migratedExits = exits, migratedEntries = entries });
        }

        private static DynamicReport MapCore(string id, string type, string? carId, string? author, DateTime created, string? plate, double lat, double lng, int score, Dictionary<string, object?> data)
            => new()
            {
                ReportId = id,
                Type = type,
                CarId = carId,
                Author = author,
                Created = created,
                CarPlate = plate,
                Latitude = lat,
                Longitude = lng,
                Score = score,
                DataJson = JsonSerializer.Serialize(data)
            };

        private static bool IsEmpty(JsonElement v) =>
            v.ValueKind == JsonValueKind.Null ||
            v.ValueKind == JsonValueKind.Undefined ||
            (v.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(v.GetString()));
    }
}
