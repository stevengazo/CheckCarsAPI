using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Controllers;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;

namespace CheckCarsAPI.Controllers;
public class PhotosControllerTests
{
    private ReportsDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ReportsDbContext(options);

        context.Photos.AddRange(
            new Photo { PhotoId = "p1", FileName = "img1.jpg", FilePath = @$"{Path.Combine(Directory.GetCurrentDirectory(), "img1.jpg")}", ReportId = "report1", DateTaken = DateTime.UtcNow },
            new Photo { PhotoId = "p2", FileName = "img2.jpg", FilePath = @$"{Path.Combine(Directory.GetCurrentDirectory(), "img2.jpg")}", ReportId = "report1", DateTaken = DateTime.UtcNow },
            new Photo { PhotoId = "p3", FileName = "img3.jpg", FilePath = @$"{Path.Combine(Directory.GetCurrentDirectory(), "img3.jpg")}", ReportId = "report2", DateTaken = DateTime.UtcNow }
        );

        context.SaveChanges();
        return context;
    }

    private PhotosController GetController(ReportsDbContext context)
    {
        var controller = new PhotosController(context);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.ControllerContext.HttpContext.Request.Scheme = "http";
        controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost");
        return controller;
    }

    [Fact]
    public async Task GetPhotos_ReturnsAll()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetPhotos();

        var actionResult = Assert.IsType<ActionResult<IEnumerable<Photo>>>(result);
        var photos = Assert.IsAssignableFrom<IEnumerable<Photo>>(actionResult.Value);
        Assert.Equal(3, photos.Count());
    }

    [Fact]
    public async Task GetPhoto_ReturnsCorrectItem()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetPhoto("p1");

        var actionResult = Assert.IsType<ActionResult<Photo>>(result);
        var photo = Assert.IsType<Photo>(actionResult.Value);
        Assert.Equal("p1", photo.PhotoId);
    }

    [Fact]
    public async Task GetPhoto_ReturnsNotFound_WhenMissing()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetPhoto("missing");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetPhotosByReport_ReturnsList()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetPhotosByReport("report1");

        var actionResult = Assert.IsType<ActionResult<List<Photo>>>(result);
        var photos = Assert.IsType<List<Photo>>(actionResult.Value);
        Assert.Equal(2, photos.Count);
        Assert.All(photos, p => Assert.StartsWith("http://localhost/images/", p.FilePath));
    }

    [Fact]
    public async Task GetPhotosByReport_ReturnsNotFound_WhenEmpty()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.GetPhotosByReport("nonexistent");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PostPhoto_CreatesItem()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var newPhoto = new Photo
        {
            PhotoId = "p100",
            FileName = "new.jpg",
            FilePath = @$"{Path.Combine(Directory.GetCurrentDirectory(), "new.jpg")}",
            ReportId = "reportX",
            DateTaken = DateTime.UtcNow
        };

        var result = await controller.PostPhoto(newPhoto);

        var actionResult = Assert.IsType<ActionResult<Photo>>(result);
        var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var photo = Assert.IsType<Photo>(created.Value);
        Assert.Equal("p100", photo.PhotoId);
    }

    [Fact]
    public async Task PutPhoto_UpdatesItem()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var updated = new Photo
        {
            PhotoId = "p1",
            FileName = "updated.jpg",
            FilePath = "C:/new/path.jpg",
            DateTaken = DateTime.UtcNow
        };

        var result = await controller.PutPhoto("p1", updated);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("updated.jpg", context.Photos.Find("p1")?.FileName);
    }

    [Fact]
    public async Task PutPhoto_ReturnsBadRequest_WhenIdMismatch()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var photo = new Photo { PhotoId = "wrong" };

        var result = await controller.PutPhoto("p1", photo);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeletePhoto_RemovesItem()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var photoPath = context.Photos.Find("p3").FilePath;
        System.IO.File.WriteAllText(photoPath, "dummy"); // create dummy file

        var result = await controller.DeletePhoto("p3");

        Assert.IsType<NoContentResult>(result);
        Assert.Null(context.Photos.Find("p3"));
    }

    [Fact]
    public async Task DeletePhoto_ReturnsNotFound_WhenMissing()
    {
        var context = GetDbContext();
        var controller = GetController(context);

        var result = await controller.DeletePhoto("nope");

        Assert.IsType<NotFoundResult>(result);
    }
}
