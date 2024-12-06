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

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

        // POST: api/EntryExitReports
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EntryExitReport>> PostEntryExitReport(EntryExitReport entryExitReport)
        {
            _context.EntryExitReports.Add(entryExitReport);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEntryExitReport", new { id = entryExitReport.ReportId }, entryExitReport);
        }

        // DELETE: api/EntryExitReports/5
        [HttpDelete("{id}")]
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
