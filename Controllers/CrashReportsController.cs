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
    public class CrashReportsController : ControllerBase
    {
        private readonly ReportsDbContext _context;

        public CrashReportsController(ReportsDbContext context)
        {
            _context = context;
        }

        // GET: api/CrashReports
        [HttpGet]

        public async Task<ActionResult<IEnumerable<CrashReport>>> GetCrashReports()
        {
            return await _context.CrashReports.ToListAsync();
        }

        // GET: api/CrashReports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CrashReport>> GetCrashReport(int id)
        {
            var crashReport = await _context.CrashReports.FindAsync(id);

            if (crashReport == null)
            {
                return NotFound();
            }

            return crashReport;
        }

        // PUT: api/CrashReports/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCrashReport(string id, CrashReport crashReport)
        {
            if (!id.Equals(crashReport.ReportId)  )
            {
                return BadRequest();
            }

            _context.Entry(crashReport).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CrashReportExists(id))
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

        // POST: api/CrashReports
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CrashReport>> PostCrashReport(CrashReport crashReport)
        {
            _context.CrashReports.Add(crashReport);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCrashReport", new { id = crashReport.ReportId }, crashReport);
        }

        // DELETE: api/CrashReports/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCrashReport(int id)
        {
            var crashReport = await _context.CrashReports.FindAsync(id);
            if (crashReport == null)
            {
                return NotFound();
            }

            _context.CrashReports.Remove(crashReport);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CrashReportExists(string id)
        {
            return _context.CrashReports.Any(e => e.ReportId.Equals( id));
        }
    }
}
