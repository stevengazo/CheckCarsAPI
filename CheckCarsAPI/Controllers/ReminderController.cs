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
        return await _reportsDbContext.Reminders.Include(e=>e.ReminderDests).ToListAsync();
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
    // GET: api/remindersbycar/5
    [HttpGet("remindersbycar/{id}")]
    public async Task<ActionResult<List<Reminder>>> RemindersByCar(int id)
    {
        List<Reminder> Reminders= await _reportsDbContext.Reminders
            .Include(e=>e.Car)
            .Where( e=> e.CarId == id)
            .OrderByDescending(E=>E.ReminderDate)
            .ToListAsync();

        if (Reminders == null)
        {
            return NotFound("Not found reminders with the Car Id: " + id.ToString());
        }
        return Reminders;
    }

    // POST: api/reminders
    [HttpPost]
    public async Task<ActionResult<Reminder>> PostReminder(Reminder reminder)
    {
        var car  = await _reportsDbContext.Cars.FirstOrDefaultAsync(e=> e.CarId == reminder.CarId);
        if (car == null)
        {
            return NotFound("Car not found");
        }
        reminder.CarId = car.CarId;
        reminder.Car = car; 
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