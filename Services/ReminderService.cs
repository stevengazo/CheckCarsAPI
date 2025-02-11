using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Data; // Ajusta el namespace según tu proyecto
using CheckCarsAPI.Services;

public class ReminderService
{
    private readonly ReportsDbContext _context;
    private readonly EmailService _emailService;

    public ReminderService(ReportsDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task CheckAndSendRemindersAsync()
    {
        var now = DateTime.UtcNow;

        // Obtener recordatorios pendientes cuyo límite ha vencido y aún no han sido enviados
        var reminders = await _context.Reminders
            .Where(r => r.ReminderDate <= now && !r.IsCompleted)
            .ToListAsync();

        foreach (var reminder in reminders)
        {
            if (!string.IsNullOrEmpty(reminder.Email))
            {
                await _emailService.SendEmailAsync(reminder.Email, reminder.Title ?? "Recordatorio", reminder.Description ?? "Tienes un recordatorio pendiente.");

                // Marcar el recordatorio como enviado
                reminder.IsCompleted = true;
                _context.Reminders.Update(reminder);
            }
        }

        await _context.SaveChangesAsync();
    }
}
