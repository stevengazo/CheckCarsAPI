using Xunit;
using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Controllers;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using Microsoft.AspNetCore.Mvc;
namespace CheckCarsAPI.Controllers;
public class CarServicesControllerTests
{
    private ReportsDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        var context = new ReportsDbContext(options);

        // Seed
        if (!context.CarsService.Any())
        {
            context.CarsService.AddRange(
                new CarService { CarServiceId = 1, Title = "Oil Change", CarId = 1, Date = DateTime.UtcNow },
                new CarService { CarServiceId = 2, Title = "Tire Replacement", CarId = 2, Date = DateTime.UtcNow }
            );
            context.SaveChanges();
        }

        return context;
    }

    [Fact]
    public async Task GetCarsService_ReturnsAll()
    {
        var context = GetDbContext();
        var controller = new CarServicesController(context);

        var result = await controller.GetCarService(context.Cars.First().CarId);

        var actionResult = Assert.IsType<ActionResult<IEnumerable<CarService>>>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<CarService>>(actionResult.Value);
        Assert.Equal(2, returnValue.Count());
    }

    [Fact]
    public async Task GetCarService_ReturnsCorrectItem()
    {
        var context = GetDbContext();
        var i = context.CarsService.FirstOrDefault();
        var controller = new CarServicesController(context);

        var result = await controller.GetCarService(i.CarId);

        var actionResult = Assert.IsType<ActionResult<CarService>>(result);
        var returnValue = Assert.IsType<CarService>(actionResult.Value);
        Assert.Equal(i.CarServiceId, returnValue.CarServiceId);
    }

    [Fact]
    public async Task GetCarService_ReturnsNotFound_WhenInvalidId()
    {
        var context = GetDbContext();
        var controller = new CarServicesController(context);

        var result = await controller.GetCarService(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PostCarService_AddsItem()
    {
        var context = GetDbContext();
        var controller = new CarServicesController(context);

        var newService = new CarService { CarServiceId = 3, Title = "Brake Check", CarId = 1, Date = DateTime.UtcNow };

        var result = await controller.PostCarService(newService);

        var actionResult = Assert.IsType<ActionResult<CarService>>(result);
        var createdAt = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var createdValue = Assert.IsType<CarService>(createdAt.Value);
        Assert.Equal("Brake Check", createdValue.Title);
    }

    [Fact]
    public async Task PutCarService_UpdatesItem()
    {
        var context = GetDbContext();
        var controller = new CarServicesController(context);

        var updatedService = new CarService { CarServiceId = 1, Title = "Oil Change Updated", CarId = 1, Date = DateTime.UtcNow };

        var result = await controller.PutCarService(1, updatedService);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Oil Change Updated", context.CarsService.Find(1)?.Title);
    }

    [Fact]
    public async Task PutCarService_ReturnsBadRequest_WhenIdMismatch()
    {
        var context = GetDbContext();
        var controller = new CarServicesController(context);

        var updatedService = new CarService { CarServiceId = 2, Title = "Mismatch", CarId = 1, Date = DateTime.UtcNow };

        var result = await controller.PutCarService(3, updatedService);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeleteCarService_RemovesItem()
    {
        var context = GetDbContext();
        var controller = new CarServicesController(context);

        var result = await controller.DeleteCarService(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(context.CarsService.Find(1));
    }

    [Fact]
    public async Task DeleteCarService_ReturnsNotFound_WhenInvalidId()
    {
        var context = GetDbContext();
        var controller = new CarServicesController(context);

        var result = await controller.DeleteCarService(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
