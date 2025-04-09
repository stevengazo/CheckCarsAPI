using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheckCarsAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReminderController : ControllerBase
{
    private readonly ReportsDbContext _reportsDbContext;

    public ReminderController(ReportsDbContext reportsDbContext)
    {
        _reportsDbContext = reportsDbContext;
    }

    // GET: api/reminders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reminder>>> GetReminders()
    {
        return await _reportsDbContext.Reminders.ToListAsync();
    }

    // GET: api/reminders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Reminder>> GetReminder(int id)
    {
        var reminder = await _reportsDbContext.Reminders.FindAsync(id);

        if (reminder == null)
        {
            return NotFound();
        }

        return reminder;
    }

    // POST: api/reminders
    [HttpPost]
    public async Task<ActionResult<Reminder>> PostReminder(Reminder reminder)
    {
        _reportsDbContext.Reminders.Add(reminder);
        await _reportsDbContext.SaveChangesAsync();

        return CreatedAtAction("GetReminder", new { id = reminder.ReminderId }, reminder);
    }

    // PUT: api/reminders/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutReminder(int id, Reminder reminder)
    {
        if (id != reminder.ReminderId)
        {
            return BadRequest();
        }

        _reportsDbContext.Entry(reminder).State = EntityState.Modified;

        try
        {
            await _reportsDbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReminderExists(id))
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

    // DELETE: api/reminders/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReminder(int id)
    {
        var reminder = await _reportsDbContext.Reminders.FindAsync(id);
        if (reminder == null)
        {
            return NotFound();
        }

        _reportsDbContext.Reminders.Remove(reminder);
        await _reportsDbContext.SaveChangesAsync();

        return NoContent();
    }

    private bool ReminderExists(int id)
    {
        return _reportsDbContext.Reminders.Any(e => e.ReminderId == id);
    }
}