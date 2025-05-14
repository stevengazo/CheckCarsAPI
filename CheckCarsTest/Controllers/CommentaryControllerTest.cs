using Xunit;
using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Controllers;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckCarsAPI.Controllers;
public class CommentaryControllerTests
{
    private ReportsDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase("TestCommentaryDb")
            .Options;

        var context = new ReportsDbContext(options);

        if (!context.commentaries.Any())
        {
            context.commentaries.AddRange(
                new Commentary { Id = 1, Text = "Comentario 1", ReportId = "rep1", Author = "user1", CreatedAt = DateTime.UtcNow },
                new Commentary { Id = 2, Text = "Comentario 2", ReportId = "rep2", Author = "user2", CreatedAt = DateTime.UtcNow }
            );
            context.SaveChanges();
        }

        return context;
    }
/*
    [Fact]
    public async Task GetCommentaries_ReturnsAllItems()
    {
        var context = GetDbContext();
        var controller = new CommentaryController(context);

        var result = await controller.getcom;

        var actionResult = Assert.IsType<ActionResult<IEnumerable<Commentary>>>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Commentary>>(actionResult.Value);
        Assert.Equal(context.commentaries.Count(), returnValue.Count());
    }*/

    [Fact]
    public async Task GetCommentary_ReturnsCorrectItem()
    {
        var context = GetDbContext();
        var controller = new CommentaryController(context);
        var i = context.commentaries.FirstOrDefault();  
        var result = await controller.GetCommentary(i.Id);

        var actionResult = Assert.IsType<ActionResult<Commentary>>(result);
        var returnValue = Assert.IsType<Commentary>(actionResult.Value);
        Assert.Equal(i.Id, returnValue.Id);
    }

    [Fact]
    public async Task GetCommentary_ReturnsNotFound_WhenInvalidId()
    {
        var context = GetDbContext();
        var controller = new CommentaryController(context);

        var result = await controller.GetCommentary(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetCommentaryByReport_ReturnsCorrectItems()
    {
        var context = GetDbContext();
        var controller = new CommentaryController(context);
        var reportId = context.commentaries.FirstOrDefault().ReportId;

        var result = await controller.GetByReport(reportId);

        var actionResult = Assert.IsType<ActionResult<List<Commentary>>>(result);
        var returnValue = Assert.IsType<List<Commentary>>(actionResult.Value);
        Assert.Single(returnValue);
        Assert.Equal(reportId, returnValue[0].ReportId);
    }

    [Fact]
    public async Task PostCommentary_AddsNewItem()
    {
        var context = GetDbContext();
        var controller = new CommentaryController(context);

        var newComment = new Commentary { Id = 3, Text = "Nuevo", Author = "user3", ReportId = "rep3", CreatedAt = DateTime.UtcNow };

        var result = await controller.PostCommentary(newComment);

        var actionResult = Assert.IsType<ActionResult<Commentary>>(result);
        var createdAt = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var createdValue = Assert.IsType<Commentary>(createdAt.Value);
        Assert.Equal("Nuevo", createdValue.Text);
    }

    [Fact]
    public async Task PutCommentary_UpdatesItem()
    {
        var context = GetDbContext();
        var controller = new CommentaryController(context);

        var updatedComment = new Commentary { Id = 1, Text = "Actualizado", Author = "user1", ReportId = "rep1", CreatedAt = DateTime.UtcNow };

        var result = await controller.PutCommentary(1, updatedComment);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Actualizado", context.commentaries.Find(1)?.Text);
    }

    [Fact]
    public async Task PutCommentary_ReturnsBadRequest_WhenIdMismatch()
    {
        var context = GetDbContext();
        var controller = new CommentaryController(context);

        var updatedComment = new Commentary { Id = 2, Text = "Error", ReportId = "rep2", Author = "user", CreatedAt = DateTime.UtcNow };

        var result = await controller.PutCommentary(3, updatedComment);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeleteCommentary_RemovesItem()
    {
        var context = GetDbContext();
        var controller = new CommentaryController(context);

        var result = await controller.DeleteCommentary(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(context.commentaries.Find(1));
    }

    [Fact]
    public async Task DeleteCommentary_ReturnsNotFound_WhenInvalidId()
    {
        var context = GetDbContext();
        var controller = new CommentaryController(context);

        var result = await controller.DeleteCommentary(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
