using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Models;
using CheckCarsAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Text.Json;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EntryExitReportsController : ControllerBase
    {
        private readonly ReportsDbContext _context;

        public EntryExitReportsController(ReportsDbContext context)
        {
            _context = context;
        }

        // GET: api/EntryExitReports
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<EntryExitReport>>> GetEntryExitReports()
        {
            //  return await _context.EntryExitReports.ToListAsync();
            return Ok(new string[] { "Value1", "Value2" });
        }

        // GET: api/EntryExitReports/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<EntryExitReport>> GetEntryExitReport(int id)
        {
            var entryExitReport = await _context.EntryExitReports.FindAsync(id);
            if (entryExitReport == null)
            {
                return NotFound();
            }
            return entryExitReport;
        }

        // PUT: api/EntryExitReports/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutEntryExitReport(string id, EntryExitReport entryExitReport)
        {
            if (!id.Equals(entryExitReport.ReportId))
            {
                return BadRequest();
            }
            _context.Entry(entryExitReport).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntryExitReportExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }


        // Declarar la lista estática para almacenar temporalmente los JSON
        private static List<EntryExitReport> tmpdu = new List<EntryExitReport>();

        [HttpPost]
        public async Task<ActionResult<string>> PostEntryExitReport([FromForm] IFormCollection formData)
        {
            try
            {
            var ReceiveFiles = formData.Files.ToList();
            var ReceiveData = formData.ToList();

            // Print each data from ReceiveData to console
            foreach (var data in ReceiveData)
            {
                Console.WriteLine($"{data.Key}: {data.Value}");
            }

            // Save files to ./images directory
            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images");
            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            foreach (var file in ReceiveFiles)
            {
                if (file.Length > 0)
                {
                var filePath = Path.Combine(imagesPath, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                }
            }

            return Ok("Datos recibidos y procesados exitosamente.");
            }
            catch (Exception e)
            {
            return BadRequest($"Error al procesar la solicitud: {e.Message}");
            }
        }



        // DELETE: api/EntryExitReports/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEntryExitReport(int id)
        {
            var entryExitReport = await _context.EntryExitReports.FindAsync(id);
            if (entryExitReport == null)
            {
                return NotFound();
            }

            _context.EntryExitReports.Remove(entryExitReport);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EntryExitReportExists(string id)
        {
            return _context.EntryExitReports.Any(e => e.ReportId.Equals(id));
        }
    }
}
