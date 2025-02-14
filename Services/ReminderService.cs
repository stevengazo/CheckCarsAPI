using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Data; // Ajusta el namespace según tu proyecto
using CheckCarsAPI.Services;
using CheckCarsAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using NuGet.Protocol;

public class ReminderService
{
    private readonly ReportsDbContext _context;
    private readonly EmailService _emailService;
    IHubContext<NotificationHub> _hub;
    private readonly IHubContext<NotificationHub> hub;

    public ReminderService(
        ReportsDbContext context,
        EmailService emailService,
        IHubContext<NotificationHub> hub
        )
    {
        _context = context;
        _emailService = emailService;
        _hub = hub;
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
                // await _emailService.SendEmailAsync(reminder.Email, reminder.Title ?? "Recordatorio", reminder.Description ?? "Tienes un recordatorio pendiente.");
                await _hub.Clients.All.SendAsync("ReceiveNotifications", reminder.ToJson());
                // Marcar el recordatorio como enviado
                reminder.IsCompleted = true;
                _context.Reminders.Update(reminder);
            }
        }

        await _context.SaveChangesAsync();
    }
}
