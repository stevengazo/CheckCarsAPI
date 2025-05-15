using Xunit;
using Moq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using CheckCarsAPI.Data;
using CheckCarsAPI.Services;
using CheckCarsAPI.Hubs;
using CheckCarsAPI.Models; // Asegúrate de que estos modelos existan
using System;

namespace CheckCarsTest.Services;
public class ReminderServiceTests
{
    [Fact]
    public async Task CheckAndSendRemindersAsync_SendsEmailAndSignalR_WhenReminderIsDue()
    {
        // Arrange: In-memory DB options
        var dbOptions = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var appDbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var reportsContext = new ReportsDbContext(dbOptions);
        using var appContext = new ApplicationDbContext(appDbOptions);

        // Fake user
        var user = new CheckCarsAPI.Models.UserApp { Id = "user123", Email = "test@example.com" };
        appContext.Users.Add(user);
        await appContext.SaveChangesAsync();

        // Fake car and reminder
        var car = new Car {CarId = 1, Brand = "Toyota", Model = "Corolla", Plate = "XYZ123" };
        var reminder = new Reminder
        {
            ReminderId = 1,
            Title = "Oil Change",
            Description = "Time for an oil change",
            Car = car,
            ReminderDate = DateTime.UtcNow,
            IsCompleted = false,
            ReminderDests = new List<ReminderDest> {
                new ReminderDest { UserId = user.Id }
            }
        };

        reportsContext.Reminders.Add(reminder);
        await reportsContext.SaveChangesAsync();

        // Mock email service
        var mockEmailService = new Mock<EmailService>();
        mockEmailService
            .Setup(s => s.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Mock SignalR
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
        var mockHub = new Mock<IHubContext<NotificationHub>>();
        mockHub.Setup(h => h.Clients).Returns(mockClients.Object);

        var service = new ReminderService(reportsContext, mockEmailService.Object, mockHub.Object, appContext);

        // Act
        await service.CheckAndSendRemindersAsync();

        // Assert
        mockEmailService.Verify(
            s => s.SendEmailAsync(user.Email, It.Is<string>(s => s.Contains("CheckCars")), It.IsAny<string>()),
            Times.Once);

        mockClientProxy.Verify(
            c => c.SendAsync("ReceiveNotifications", It.IsAny<string>(), default),
            Times.Once);

        var updatedReminder = await reportsContext.Reminders.FirstAsync();
        Assert.True(updatedReminder.SendIt);
    }
}
