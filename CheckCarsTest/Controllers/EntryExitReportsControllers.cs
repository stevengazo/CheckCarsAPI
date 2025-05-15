using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using CheckCarsAPI.Controllers;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using CheckCarsAPI.Services;
using Moq;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
namespace CheckCarsAPI.Controllers;
public class EntryExitReportsControllerTests
{
    private ReportsDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ReportsDbContext(options);

        context.EntryExitReports.AddRange(
            new EntryExitReport
            {
                ReportId = "entry1",
                CarPlate = "ABC123",
                Author = "user1",
                Created = DateTime.UtcNow
            },
            new EntryExitReport
            {
                ReportId = "entry2",
                CarPlate = "XYZ789",
                Author = "user2",
                Created = DateTime.UtcNow.AddDays(-1)
            }
        );

        context.SaveChanges();
        return context;
    }

    private EntryExitReportsController GetController(ReportsDbContext context)
    {
        var mockEmailService = new Mock<EmailService>(new Mock<IConfiguration>().Object);
        return new EntryExitReportsController(context, mockEmailService.Object);
    }

    [Fact]
    public async Task GetEntryExitReports_ReturnsAll()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetEntryExitReports();

        var actionResult = Assert.IsType<ActionResult<IEnumerable<EntryExitReport>>>(result);
        var reports = Assert.IsAssignableFrom<IEnumerable<EntryExitReport>>(actionResult.Value);
        Assert.Equal(2, reports.Count());
    }

    [Fact]
    public async Task GetEntryExitReport_ReturnsCorrectItem()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetEntryExitReport("entry1");

        var actionResult = Assert.IsType<ActionResult<EntryExitReport>>(result);
        var report = Assert.IsType<EntryExitReport>(actionResult.Value);
        Assert.Equal("entry1", report.ReportId);
    }

    [Fact]
    public async Task GetEntryExitReport_ReturnsNotFound_WhenInvalidId()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetEntryExitReport("invalid");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PostEntryExitReport_CreatesNewItem()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var newReport = new EntryExitReport
        {
            ReportId = "entry3",
            CarPlate = "NEW123",
            Author = "newuser",
            Created = DateTime.UtcNow
        };

        var result = await controller.PostEntryExitReport(newReport);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var report = Assert.IsType<EntryExitReport>(created.Value);
        Assert.Equal("entry3", report.ReportId);
    }

    [Fact]
    public async Task PostEntryExitReport_ReturnsConflict_IfExists()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var duplicate = new EntryExitReport
        {
            ReportId = "entry1",
            CarPlate = "DUPLICATE",
            Created = DateTime.UtcNow
        };

        var result = await controller.PostEntryExitReport(duplicate);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("The report already exists", conflict.Value);
    }

    [Fact]
    public async Task PutEntryExitReport_UpdatesItem()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var updated = new EntryExitReport
        {
            ReportId = "entry1",
            CarPlate = "UPDATED",
            Author = "user1",
            Created = DateTime.UtcNow
        };

        var result = await controller.PutEntryExitReport("entry1", updated);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("UPDATED", context.EntryExitReports.Find("entry1")?.CarPlate);
    }

    [Fact]
    public async Task PutEntryExitReport_ReturnsBadRequest_WhenIdMismatch()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var invalidUpdate = new EntryExitReport
        {
            ReportId = "wrong-id",
            CarPlate = "BADID",
            Created = DateTime.UtcNow
        };

        var result = await controller.PutEntryExitReport("entry1", invalidUpdate);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeleteEntryExitReport_RemovesItem()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.DeleteEntryExitReport("entry1");

        Assert.IsType<NoContentResult>(result);
        Assert.Null(context.EntryExitReports.Find("entry1"));
    }

    [Fact]
    public async Task DeleteEntryExitReport_ReturnsNotFound_WhenInvalidId()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.DeleteEntryExitReport("not_exist");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetSearchExitReports_FiltersByPlate()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var i = context.EntryExitReports.FirstOrDefault();

        var result = await controller.GetSearchExitReports(plate: i.CarPlate);

        var actionResult = Assert.IsType<ActionResult<IEnumerable<EntryExitReport>>>(result);
        var filtered = Assert.IsAssignableFrom<IEnumerable<EntryExitReport>>(actionResult.Value);
        Assert.Single(filtered);
        Assert.Equal(i.CarPlate, filtered.First().CarPlate);
    }
    [Fact]
    public async Task GetSearchExitReports_FiltersByDate()
    {
        var context = GetDbContext();
        var controller = GetController(context);
        var i = context.EntryExitReports.FirstOrDefault();

        var result = await controller.GetSearchExitReports(date: i.Created.Date);

        var actionResult = Assert.IsType<ActionResult<IEnumerable<EntryExitReport>>>(result);
        var filtered = Assert.IsAssignableFrom<IEnumerable<EntryExitReport>>(actionResult.Value);
        Assert.All(filtered, x => Assert.Equal(i.Created.Date, x.Created.Date));
    }
    [Fact]
    public async Task GetSearchExitReports_FiltersByCarId()
    {
        // Arrange
        var context = GetDbContext();

        var testReport = new EntryExitReport
        {
            CarPlate = "XYZ123",
            CarId = "123",
            Author = "CarIdTester",
            Created = DateTime.UtcNow
        };
        context.EntryExitReports.Add(testReport);
        context.SaveChanges();

        var controller = GetController(context);

        // Act
        var result = await controller.GetSearchExitReports(carId: testReport.CarId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var filtered = Assert.IsAssignableFrom<IEnumerable<EntryExitReport>>(okResult.Value);

        Assert.NotEmpty(filtered);
        Assert.All(filtered, x => Assert.Equal(testReport.CarId, x.CarId));
    }
    [Fact]
    public async Task GetSearchExitReports_FiltersByAuthor()
    {
        // Arrange
        var context = GetDbContext();

        var testReport = new EntryExitReport
        {
            CarPlate = "AUTH123",
            CarId = "fds",
            Author = "AuthorTest",
            Created = DateTime.UtcNow
        };

        context.EntryExitReports.Add(testReport);
        context.SaveChanges();

        var controller = GetController(context);

        // Act
        var result = await controller.GetSearchExitReports(author: testReport.Author);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var filtered = Assert.IsAssignableFrom<IEnumerable<EntryExitReport>>(okResult.Value);

        Assert.NotEmpty(filtered);
        Assert.All(filtered, x => Assert.Equal(testReport.Author, x.Author));
    }


    [Fact]
    public async Task GetSearchExitReports_FiltersByAllParameters()
    {
        // Arrange
        var context = GetDbContext();

        // Asegurarse de que haya datos conocidos
        var testReport = new EntryExitReport
        {
            CarPlate = "TEST123",
            CarId = "999",
            Author = "JaneDoe",
            Created = DateTime.UtcNow
        };

        context.EntryExitReports.Add(testReport);
        context.SaveChanges();

        var controller = GetController(context);

        // Act
        var result = await controller.GetSearchExitReports(
            date: testReport.Created.Date,
            plate: testReport.CarPlate,
            carId: testReport.CarId,
            author: testReport.Author);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var filtered = Assert.IsAssignableFrom<IEnumerable<EntryExitReport>>(okResult.Value);

        Assert.NotEmpty(filtered);
        Assert.All(filtered, x =>
        {
            Assert.Equal(testReport.CarPlate, x.CarPlate);
            Assert.Equal(testReport.CarId, x.CarId);
            Assert.Equal(testReport.Author, x.Author);
            Assert.Equal(testReport.Created.Date, x.Created.Date);
        });
    }

    [Fact]
    public async Task GetSearchExitReports_ReturnsBadRequest_IfNoParams()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetSearchExitReports();

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Debe proporcionar al menos un parámetro de búsqueda.", badRequest.Value);
    }



}
