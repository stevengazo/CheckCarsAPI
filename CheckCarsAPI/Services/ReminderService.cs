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

        var startDate = now.Date.AddDays(-5);
        var endDate = now.Date.AddDays(2);

        var reminders = await _context.Reminders
            .Include(e => e.Car)
            .Include(e => e.ReminderDests)
            .Where(r => r.ReminderDate.Date >= startDate
                     && r.ReminderDate.Date <= endDate
                     && !r.IsCompleted)
            .ToListAsync();


        // recorre los recordatorios
        foreach (var reminder in reminders)
        {
            // si el recordatorio tiene destinatarios, envía el correo a cada uno de ellos
            if (reminder.ReminderDests != null && reminder.ReminderDests.Count > 0)
            {
                foreach (var dest in reminder.ReminderDests)
                {
                    var user = _applicationDbContext.Users.FirstOrDefault(e => e.Id == dest.UserId);
                    if (user != null)
                    {
                        string subject = $"Recordatorio CheckCars - {reminder.Title ?? "Sin Titulo"}";
                        string htmlMessage = $@"
                                    <html>
                                    <head>
                                        <style>
                                            body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                                            .container {{ padding: 20px; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; }}
                                            .header {{ font-size: 20px; font-weight: bold; margin-bottom: 20px; }}
                                            .content p {{ margin: 5px 0; }}
                                            .footer {{ margin-top: 20px; font-size: 12px; color: #888; }}
                                        </style>
                                    </head>
                                    <body>
                                        <div class='container'>
                                            <div class='header'>🔔 Recordatorio pendiente - CheckCars</div>
                                            <div class='content'>
                                                <p><strong>Título:</strong> {reminder.Title ?? "Sin título"}</p>
                                                <p><strong>Vehículo:</strong> {reminder.Car.Brand} {reminder.Car.Model} ({reminder.Car.Plate})</p>
                                                <p><strong>Fecha:</strong> {reminder.ReminderDate:dd/MM/yyyy}</p>
                                                <p><strong>Descripción:</strong></p>
                                                <p>{reminder.Description ?? "Sin descripción."}</p>
                                            </div>
                                            <div class='footer'>
                                                Gracias por usar CheckCars.
                                            </div>
                                        </div>
                                    </body>
                                    </html>";

                        await _emailService.SendEmailAsync(user.Email, subject, htmlMessage);
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
