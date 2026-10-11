using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,User,Guest")]
    public class FormDefinitionsController : ControllerBase
    {
        private readonly ReportsDbContext _context;

        public FormDefinitionsController(ReportsDbContext context)
        {
            _context = context;
        }

        /// <summary>Lista todas las definiciones, o la activa de un tipo con ?type=Exit.</summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? type = null)
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                var active = await _context.FormDefinitions
                    .Include(f => f.Fields)
                    .Include(f => f.PhotoSlots)
                    .Where(f => f.Type == type && f.IsActive)
                    .OrderByDescending(f => f.Version)
                    .FirstOrDefaultAsync();
                return active == null ? NotFound() : Ok(active);
            }

            var all = await _context.FormDefinitions
                .Include(f => f.Fields)
                .Include(f => f.PhotoSlots)
                .ToListAsync();
            return Ok(all);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var def = await _context.FormDefinitions
                .Include(f => f.Fields)
                .Include(f => f.PhotoSlots)
                .FirstOrDefaultAsync(f => f.FormDefinitionId == id);
            return def == null ? NotFound() : Ok(def);
        }

        /// <summary>Crea/actualiza la definición activa de un tipo (nueva versión).</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Save(FormDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(definition.Type))
                return BadRequest("El tipo es obligatorio.");

            // Desactiva las definiciones previas del mismo tipo.
            var previous = await _context.FormDefinitions
                .Where(f => f.Type == definition.Type && f.IsActive)
                .ToListAsync();
            var nextVersion = previous.Count > 0 ? previous.Max(f => f.Version) + 1 : 1;
            previous.ForEach(f => f.IsActive = false);

            definition.FormDefinitionId = Guid.NewGuid().ToString();
            definition.Version = nextVersion;
            definition.IsActive = true;
            definition.UpdatedAt = DateTime.UtcNow;

            _context.FormDefinitions.Add(definition);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = definition.FormDefinitionId }, definition);
        }
    }
}
