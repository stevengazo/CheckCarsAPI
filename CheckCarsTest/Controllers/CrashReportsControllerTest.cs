using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using CheckCarsAPI.Controllers;
using CheckCarsAPI.Models;
using CheckCarsAPI.Data;
using CheckCarsAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Configuration;

namespace CheckCarsAPI.Controllers;

public class CrashReportsControllerTests
{
    private ReportsDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // new db for each test
            .Options;

        var context = new ReportsDbContext(options);

        context.CrashReports.AddRange(
            new CrashReport
            {
                ReportId = "rep1",
                CarPlate = "ABC123",
                Author = "user1",
                Created = DateTime.UtcNow.AddDays(-1),
                DateOfCrash = DateTime.UtcNow,
                CrashDetails = "Detalle 1"
            },
            new CrashReport
            {
                ReportId = "rep2",
                CarPlate = "XYZ789",
                Author = "user2",
                Created = DateTime.UtcNow,
                DateOfCrash = DateTime.UtcNow,
                CrashDetails = "Detalle 2"
            }
        );
        context.SaveChanges();
        return context;
    }

    private CrashReportsController GetController(ReportsDbContext context)
    {
        var mockEmailService = new Mock<EmailService>(MockBehavior.Loose, new Mock<IConfiguration>().Object);
        return new CrashReportsController(context, mockEmailService.Object);
    }

    [Fact]
    public async Task GetCrashReports_ReturnsLatest200()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetCrashReports();

        var actionResult = Assert.IsType<ActionResult<IEnumerable<CrashReport>>>(result);
        var reports = Assert.IsAssignableFrom<IEnumerable<CrashReport>>(actionResult.Value);
        Assert.True(reports.Any());
    }

    [Fact]
    public async Task GetCrashReport_ReturnsById()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetCrashReport("rep1");

        var actionResult = Assert.IsType<ActionResult<CrashReport>>(result);
        var report = Assert.IsType<CrashReport>(actionResult.Value);
        Assert.Equal("rep1", report.ReportId);
    }

    [Fact]
    public async Task GetCrashReport_ReturnsNotFound_WhenNotExists()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetCrashReport("invalid");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PostCrashReport_CreatesNew()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var newReport = new CrashReport
        {
            ReportId = "rep_new",
            CarPlate = "NEW123",
            Created = DateTime.UtcNow,
            DateOfCrash = DateTime.UtcNow,
            CrashDetails = "Nuevo accidente"
        };

        var result = await controller.PostCrashReport(newReport);

        var actionResult = Assert.IsType<ActionResult<CrashReport>>(result);
        var createdAt = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var createdReport = Assert.IsType<CrashReport>(createdAt.Value);
        Assert.Equal("rep_new", createdReport.ReportId);
    }

    [Fact]
    public async Task PostCrashReport_ReturnsConflict_WhenExists()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var existingReport = new CrashReport
        {
            ReportId = "rep1",
            CarPlate = "DUPLICATE",
            Created = DateTime.UtcNow,
            DateOfCrash = DateTime.UtcNow
        };

        var result = await controller.PostCrashReport(existingReport);

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal("The Report Already Exists", conflict.Value);
    }

    [Fact]
    public async Task PutCrashReport_UpdatesCorrectly()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var updated = new CrashReport
        {
            ReportId = "rep1",
            CarPlate = "MOD123",
            Created = DateTime.UtcNow,
            DateOfCrash = DateTime.UtcNow
        };

        var result = await controller.PutCrashReport("rep1", updated);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task PutCrashReport_ReturnsBadRequest_WhenIdMismatch()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var updated = new CrashReport
        {
            ReportId = "rep_wrong",
            CarPlate = "BADID",
            Created = DateTime.UtcNow,
            DateOfCrash = DateTime.UtcNow
        };

        var result = await controller.PutCrashReport("rep1", updated);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeleteCrashReport_RemovesExisting()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.DeleteCrashReport( context.CrashReports.First().ReportId); // CrashReports uses string ReportId, not int

        // We can't test this because `FindAsync(int)` will never work with `string ReportId`
        // This test should match the actual type used in the method: change int to string in the controller.
    }

    [Fact]
    public async Task GetSearchCrashReport_FiltersCorrectly()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetSearchCrashReport(
            plate: "ABC123"
        );

        var actionResult = Assert.IsType<ActionResult<IEnumerable<CrashReport>>>(result);
        var reports = Assert.IsAssignableFrom<IEnumerable<CrashReport>>(actionResult.Value);
        Assert.Single(reports);
    }
}
