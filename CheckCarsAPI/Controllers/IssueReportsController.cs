using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CheckCars.Models;
using CheckCarsAPI.Data;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IssueReportsController : ControllerBase
    {
        private readonly ReportsDbContext _context;

        public IssueReportsController(ReportsDbContext context)
        {
            _context = context;
        }

        // GET: api/IssueReports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IssueReport>>> GetIssueReports()
        {
            return await _context.IssueReports.ToListAsync();
        }

        // GET: api/IssueReports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IssueReport>> GetIssueReport(int id)
        {
            var issueReport = await _context.IssueReports.FindAsync(id);

            if (issueReport == null)
            {
                return NotFound();
            }

            return issueReport;
        }

        // PUT: api/IssueReports/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIssueReport(int id, IssueReport issueReport)
        {
            if (id != issueReport.ReportId)
            {
                return BadRequest();
            }

            _context.Entry(issueReport).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IssueReportExists(id))
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

        // POST: api/IssueReports
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<IssueReport>> PostIssueReport(IssueReport issueReport)
        {
            _context.IssueReports.Add(issueReport);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIssueReport", new { id = issueReport.ReportId }, issueReport);
        }

        // DELETE: api/IssueReports/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssueReport(int id)
        {
            var issueReport = await _context.IssueReports.FindAsync(id);
            if (issueReport == null)
            {
                return NotFound();
            }

            _context.IssueReports.Remove(issueReport);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IssueReportExists(int id)
        {
            return _context.IssueReports.Any(e => e.ReportId == id);
        }
    }
}
