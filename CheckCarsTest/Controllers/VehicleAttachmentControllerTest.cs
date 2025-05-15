using CheckCarsAPI.Controllers;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using CheckCarsAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CheckCarsAPI.Controllers;

public class VehicleAttachmentsControllerTests
{
    private ReportsDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: "VehicleAttachmentsTestDb")
            .Options;

        var context = new ReportsDbContext(options);

        if (!context.Cars.Any())
        {
            var car = new Car { CarId = "1", Plate = "ABC123", Brand = "TestBrand" };
            context.Cars.Add(car);
            context.VehicleAttachments.Add(new VehicleAttachment
            {
                AttachmentId = "att1",
                FileName = "file.txt",
                FilePath = "folder/file.txt",
                CarId = "1",
                Car = car
            });
            context.SaveChanges();
        }

        return context;
    }

    [Fact]
    public async Task GetAttachments_ReturnsAll()
    {
        var context = GetDbContext();
        var fileService = new Mock<IFileService>();

        var controller = new VehicleAttachmentsController(context, fileService.Object);
        var result = await controller.GetAttachment(context.Cars.First().CarId.ToString());

        var actionResult = Assert.IsType<ActionResult<IEnumerable<VehicleAttachment>>>(result);
        var attachments = Assert.IsType<List<VehicleAttachment>>(actionResult.Value);
        Assert.Single(attachments);
    }

    [Fact]
    public async Task GetAttachment_ReturnsSingle_WhenExists()
    {
        var context = GetDbContext();
        var fileService = new Mock<IFileService>();

        var controller = new VehicleAttachmentsController(context, fileService.Object);
        var result = await controller.GetAttachment("att1");

        var actionResult = Assert.IsType<ActionResult<VehicleAttachment>>(result);
        var attachment = Assert.IsType<VehicleAttachment>(actionResult.Value);
        Assert.Equal("file.txt", attachment.FileName);
    }

    [Fact]
    public async Task GetAttachment_ReturnsNotFound_WhenMissing()
    {
        var context = GetDbContext();
        var fileService = new Mock<IFileService>();

        var controller = new VehicleAttachmentsController(context, fileService.Object);
        var result = await controller.GetAttachment("missing");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task UploadAttachment_ReturnsCreated_WhenValid()
    {
        var context = GetDbContext();
        var fileService = new Mock<IFileService>();
        fileService.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("folder/newfile.txt");

        var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("test")), 0, 4, "Data", "newfile.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var controller = new VehicleAttachmentsController(context, fileService.Object);
        var result = await controller.UploadAttachment("1", file);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result);
        var attachment = Assert.IsType<VehicleAttachment>(createdAt.Value);
        Assert.Equal("newfile.txt", attachment.FileName);
    }

    [Fact]
    public async Task UploadAttachment_ReturnsBadRequest_WhenFileMissing()
    {
        var context = GetDbContext();
        var fileService = new Mock<IFileService>();
        var controller = new VehicleAttachmentsController(context, fileService.Object);

        var result = await controller.UploadAttachment("1", null);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File not selected", badRequest.Value);
    }

    [Fact]
    public async Task DeleteAttachment_ReturnsNoContent_WhenDeleted()
    {
        var context = GetDbContext();
        var fileService = new Mock<IFileService>();
        fileService.Setup(f => f.DeleteFile(It.IsAny<string>())).Returns(true);

        var controller = new VehicleAttachmentsController(context, fileService.Object);
        var result = await controller.DeleteAttachment("att1");

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteAttachment_ReturnsNotFound_WhenMissing()
    {
        var context = GetDbContext();
        var fileService = new Mock<IFileService>();
        var controller = new VehicleAttachmentsController(context, fileService.Object);

        var result = await controller.DeleteAttachment("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAttachment_ReturnsError_WhenFileDeleteFails()
    {
        var context = GetDbContext();
        var fileService = new Mock<IFileService>();
        fileService.Setup(f => f.DeleteFile(It.IsAny<string>())).Returns(false);

        var controller = new VehicleAttachmentsController(context, fileService.Object);
        var result = await controller.DeleteAttachment("att1");

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, error.StatusCode);
    }
}
