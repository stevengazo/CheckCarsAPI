using CheckCarsAPI.Controllers;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CheckCarsAPI.Controllers;
public class ReminderControllerTests
{
    private ReportsDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_RemindersDb")
            .Options;
        var context = new ReportsDbContext(options);

        if (!context.Reminders.Any())
        {
            context.Reminders.AddRange(
                new Reminder { ReminderId = 1, Title = "Reminder 1" },
                new Reminder { ReminderId = 2, Title = "Reminder 2" }
            );
            context.SaveChanges();
        }

        return context;
    }

    [Fact]
    public async Task GetReminders_ReturnsAll()
    {
        var context = GetDbContext();
        var controller = new ReminderController(context);

        var result = await controller.GetReminders();

        var actionResult = Assert.IsType<ActionResult<IEnumerable<Reminder>>>(result);
        var reminders = Assert.IsType<List<Reminder>>(actionResult.Value);
        Assert.Equal(2, reminders.Count);
    }

    [Fact]
    public async Task GetReminder_ReturnsExisting()
    {
        var context = GetDbContext();
        var controller = new ReminderController(context);

        var result = await controller.GetReminder(1);

        var actionResult = Assert.IsType<ActionResult<Reminder>>(result);
        var reminder = Assert.IsType<Reminder>(actionResult.Value);
        Assert.Equal("Reminder 1", reminder.Title);
    }

    [Fact]
    public async Task GetReminder_ReturnsNotFound()
    {
        var context = GetDbContext();
        var controller = new ReminderController(context);

        var result = await controller.GetReminder(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PostReminder_AddsNew()
    {
        var context = GetDbContext();
        var controller = new ReminderController(context);

        var newReminder = new Reminder { Title = "New Reminder" };

        var result = await controller.PostReminder(newReminder);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        var reminder = Assert.IsType<Reminder>(createdAt.Value);
        Assert.Equal("New Reminder", reminder.Title);
    }

    [Fact]
    public async Task PutReminder_UpdatesExisting()
    {
        var context = GetDbContext();
        var controller = new ReminderController(context);

        var updated = new Reminder { ReminderId = 1, Title = "Updated Reminder" };
        var result = await controller.PutReminder(1, updated);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Updated Reminder", context.Reminders.Find(1)?.Title);
    }

    [Fact]
    public async Task PutReminder_ReturnsBadRequest_IfIdMismatch()
    {
        var context = GetDbContext();
        var controller = new ReminderController(context);

        var updated = new Reminder { ReminderId = 5, Title = "Mismatch" };

        var result = await controller.PutReminder(1, updated);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeleteReminder_RemovesExisting()
    {
        var context = GetDbContext();
        var controller = new ReminderController(context);

        var result = await controller.DeleteReminder(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(context.Reminders.Find(1));
    }

    [Fact]
    public async Task DeleteReminder_ReturnsNotFound()
    {
        var context = GetDbContext();
        var controller = new ReminderController(context);

        var result = await controller.DeleteReminder(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
