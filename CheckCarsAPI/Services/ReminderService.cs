using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Data; // Ajusta el namespace según tu proyecto
using CheckCarsAPI.Services;

public class ReminderService
{
    private readonly ReportsDbContext _context;
    private readonly EmailService _emailService;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly NotificationService _notificationService;

    public ReminderService(
        ReportsDbContext context,
        EmailService emailService,
        NotificationService notificationService,
        ApplicationDbContext applicationDbContext
        )
    {
        _context = context;
        _emailService = emailService;
        _notificationService = notificationService;
        _applicationDbContext = applicationDbContext;
    }

    public async Task CheckAndSendRemindersAsync()
    {
        try
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
                var vehicleLabel = reminder.Car != null
                    ? $"{reminder.Car.Brand} {reminder.Car.Model} ({reminder.Car.Plate})"
                    : "Vehículo";

                // si el recordatorio tiene destinatarios, envía el correo y la
                // notificación (persistida + en vivo) a cada uno de ellos
                if (reminder.ReminderDests != null && reminder.ReminderDests.Count > 0)
                {
                    foreach (var dest in reminder.ReminderDests)
                    {
                        if (string.IsNullOrWhiteSpace(dest.UserId))
                        {
                            continue;
                        }

                        var user = _applicationDbContext.Users.FirstOrDefault(e => e.Id == dest.UserId);
                        if (user == null)
                        {
                            continue;
                        }

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
                                                <p><strong>Vehículo:</strong> {vehicleLabel}</p>
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

                        if (!string.IsNullOrWhiteSpace(user.Email))
                        {
                            await _emailService.SendEmailAsync(user.Email, subject, htmlMessage);
                        }

                        await _notificationService.NotifyUserAsync(
                            userId: dest.UserId,
                            title: reminder.Title ?? "Recordatorio",
                            message: $"{vehicleLabel} · {reminder.ReminderDate:dd/MM/yyyy}. {reminder.Description ?? string.Empty}".Trim(),
                            type: "Reminder",
                            url: $"/cars/{reminder.CarId}");
                    }
                }

                // Marcar el recordatorio como enviado
                reminder.SendIt = true;
                _context.Reminders.Update(reminder);
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception er)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(er.Message);
            Console.ResetColor();
        }
    }
}
