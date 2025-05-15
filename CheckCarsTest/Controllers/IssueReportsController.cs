using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using CheckCarsAPI.Controllers;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckCarsAPI.Controllers;
public class IssueReportsControllerTests
{
    private ReportsDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ReportsDbContext(options);
        context.IssueReports.AddRange(
            new IssueReport
            {
                ReportId = "issue1",
                CarPlate = "ABC123",
                Author = "user1",
                Created = DateTime.UtcNow,
                Priority = "High"
            },
            new IssueReport
            {
                ReportId = "issue2",
                CarPlate = "XYZ789",
                Author = "user2",
                Created = DateTime.UtcNow.AddDays(-1),
                Priority = "Low"
            }
        );
        context.SaveChanges();
        return context;
    }

    private IssueReportsController GetController(ReportsDbContext context)
    {
        return new IssueReportsController(context);
    }

    [Fact]
    public async Task GetIssueReports_ReturnsAll()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetIssueReports();

        var actionResult = Assert.IsType<ActionResult<IEnumerable<IssueReport>>>(result);
        var value = Assert.IsAssignableFrom<IEnumerable<IssueReport>>(actionResult.Value);
        Assert.Equal(2, value.Count());
    }

    [Fact]
    public async Task GetIssueReport_ReturnsCorrectItem()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetIssueReport("issue1");

        var actionResult = Assert.IsType<ActionResult<IssueReport>>(result);
        var report = Assert.IsType<IssueReport>(actionResult.Value);
        Assert.Equal("issue1", report.ReportId);
    }

    [Fact]
    public async Task GetIssueReport_ReturnsNotFound_WhenInvalidId()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetIssueReport("not_found");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PostIssueReport_CreatesNew()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var newReport = new IssueReport
        {
            ReportId = "issue3",
            CarPlate = "NEW123",
            Author = "user3",
            Created = DateTime.UtcNow,
            Priority = "Medium"
        };

        var result = await controller.PostIssueReport(newReport);

        Assert.IsType<OkResult>(result.Result);
        Assert.Equal(3, context.IssueReports.Count());
    }

    [Fact]
    public async Task PostIssueReport_ReturnsConflict_IfExists()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var existing = new IssueReport
        {
            ReportId = "issue1",
            CarPlate = "SAME",
            Created = DateTime.UtcNow
        };

        var result = await controller.PostIssueReport(existing);

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal("The Report Already Exists", conflict.Value);
    }

    [Fact]
    public async Task PutIssueReport_UpdatesCorrectly()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var i = context.IssueReports.FirstOrDefault();


        var updated = new IssueReport
        {
            ReportId = i.ReportId,
            CarPlate = "MODIFIED",
            Author = "user1",
            Created = DateTime.UtcNow
        };

        var result = await controller.PutIssueReport(i.ReportId, updated);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("MODIFIED", context.IssueReports.Find(i.ReportId)?.CarPlate);
    }

    [Fact]
    public async Task PutIssueReport_ReturnsBadRequest_WhenIdMismatch()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var updated = new IssueReport
        {
            ReportId = "issue_different",
            CarPlate = "INVALID"
        };

        var result = await controller.PutIssueReport("issue1", updated);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeleteIssueReport_RemovesItem()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.DeleteIssueReport("issue1");

        Assert.IsType<NoContentResult>(result);
        Assert.Null(context.IssueReports.Find("issue1"));
    }

    [Fact]
    public async Task DeleteIssueReport_ReturnsNotFound_WhenInvalidId()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.DeleteIssueReport("missing");

        Assert.IsType<NotFoundResult>(result);
    }
    [Fact]
    public async Task GetSearchIssueReports_ReturnsFilteredByPlate()
    {
        // Arrange
        var context = GetDbContext();

        var testReport = context.IssueReports.FirstOrDefault();
       
        var controller = GetController(context);

        // Act
        var result = await controller.GetSearchIssueReports(plate: testReport.CarPlate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var filtered = Assert.IsAssignableFrom<IEnumerable<IssueReport>>(okResult.Value);

        Assert.Single(filtered);
        Assert.Equal(testReport.CarPlate, filtered.First().CarPlate);
    }

}
