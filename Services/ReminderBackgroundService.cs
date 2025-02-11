using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace CheckCarsAPI.Services;

public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderBackgroundService> _logger;

    public ReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var reminderService = scope.ServiceProvider.GetRequiredService<ReminderService>();
                await reminderService.CheckAndSendRemindersAsync();
            }

            _logger.LogInformation("Verificación de recordatorios ejecutada.");
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Verificar cada 10 minutos
        }
    }
}
