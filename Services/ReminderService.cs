using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Data; // Ajusta el namespace según tu proyecto
using CheckCarsAPI.Services;
using CheckCarsAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using NuGet.Protocol;
using Newtonsoft.Json;

public class ReminderService
{
    private readonly ReportsDbContext _context;
    private readonly EmailService _emailService;
    private readonly ApplicationDbContext _applicationDbContext;
    IHubContext<NotificationHub> _hub;
    private readonly IHubContext<NotificationHub> hub;

    public ReminderService(
        ReportsDbContext context,
        EmailService emailService,
        IHubContext<NotificationHub> hub,
        ApplicationDbContext applicationDbContext
        )
    {
        _context = context;
        _emailService = emailService;
        _hub = hub;
        _applicationDbContext = applicationDbContext;
    }

    public async Task CheckAndSendRemindersAsync()
    {
        var now = DateTime.UtcNow;

        // Obtener recordatorios pendientes cuyo límite ha vencido y aún no han sido enviados
        var reminders = await _context.Reminders.Include(e => e.ReminderDests)
            .Where(r => r.ReminderDate <= now && !r.IsCompleted)
            .ToListAsync();

        // recorre los recordatorios
        foreach (var reminder in reminders)
        {
            // si el recordatorio tiene destinatarios, envía el correo a cada uno de ellos
            if (reminder.ReminderDests != null && reminder.ReminderDests.Count > 0)
            {
                foreach (var dest in reminder.ReminderDests)
                {
                    // obtiene el usuario por su ID
                    var user = _applicationDbContext.Users.FirstOrDefault(e => e.Id == dest.UserId);
                    if (user != null)
                    {
                        await _emailService.SendEmailAsync(user.Email, reminder.Title ?? "Recordatorio  - CheckCars", reminder.Description ?? "Tienes un recordatorio pendiente.");
                    }
                }
            }

            var json = JsonConvert.SerializeObject(reminder, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            await _hub.Clients.All.SendAsync("ReceiveNotifications", json);

            // Marcar el recordatorio como enviado
            reminder.SendIt = true;
            _context.Reminders.Update(reminder);
        }

        await _context.SaveChangesAsync();
    }
}
