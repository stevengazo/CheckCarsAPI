using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Controllers;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CheckCarsAPI.Controllers
{
    public class CarsControllerTests
    {
        private ReportsDbContext GetDbContextWithData()
        {
            var options = new DbContextOptionsBuilder<ReportsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestCarsDb")
                .Options;

            var context = new ReportsDbContext(options);

            // Agregamos datos de ejemplo
            if (!context.Cars.Any())
            {
                context.Cars.AddRange(
                    new Car { CarId = "fds", Plate = "AAA123", Model = "Toyota" },
                    new Car { CarId = "dfs", Plate = "BBB456", Model = "Honda" }
                );
                context.SaveChanges();
            }

            return context;
        }

        [Fact]
        public async Task GetCars_ReturnsAllCars()
        {
            // Arrange
            var context = GetDbContextWithData();
            var controller = new CarsController(context);

            // Act
            var result = await controller.GetCars();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Car>>>(result);
            var cars = Assert.IsAssignableFrom<IEnumerable<Car>>(actionResult.Value);
            Assert.Equal(2, cars.Count());
        }

        [Fact]
        public async Task GetCar_ReturnsCorrectCar()
        {
            var context = GetDbContextWithData();
            var controller = new CarsController(context);

            var result = await controller.GetCar("1");

            var actionResult = Assert.IsType<ActionResult<Car>>(result);
            var car = Assert.IsType<Car>(actionResult.Value);
            Assert.Equal("Toyota", car.Model);
        }

        [Fact]
        public async Task GetCar_ReturnsNotFound_ForInvalidId()
        {
            var context = GetDbContextWithData();
            var controller = new CarsController(context);

            var result = await controller.GetCar("99");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostCar_AddsNewCar()
        {
            var context = GetDbContextWithData();
            var controller = new CarsController(context);
            var newCar = new Car { CarId = "3", Plate = "CCC789", Model = "Mazda" };

            var result = await controller.PostCar(newCar);

            var actionResult = Assert.IsType<ActionResult<Car>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var car = Assert.IsType<Car>(createdAtActionResult.Value);
            Assert.Equal("Mazda", car.Model);
        }

        [Fact]
        public async Task DeleteCar_RemovesCar()
        {
            var context = GetDbContextWithData();
            var controller = new CarsController(context);

            var result = await controller.DeleteCar("1");

            Assert.IsType<NoContentResult>(result);
            Assert.Null(await context.Cars.FindAsync("1"));
        }

        [Fact]
        public async Task PutCar_UpdatesExistingCar()
        {
            var context = GetDbContextWithData();
            var controller = new CarsController(context);
            var updatedCar = new Car { CarId = "2", Plate = "UPDATED456", Model = "UpdatedHonda" };

            var result = await controller.PutCar("2", updatedCar);

            Assert.IsType<NoContentResult>(result);
            var car = await context.Cars.FindAsync(2);
            Assert.Equal("UpdatedHonda", car.Model);
            Assert.Equal("UPDATED456", car.Plate);
        }

        [Fact]
        public async Task PutCar_ReturnsBadRequest_WhenIdMismatch()
        {
            var context = GetDbContextWithData();
            var controller = new CarsController(context);
            var updatedCar = new Car { CarId = "2", Plate = "Mismatch", Model = "Mismatch" };

            var result = await controller.PutCar("2", updatedCar);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task GetCarsPlates_ReturnsFormattedPlates()
        {
            var context = GetDbContextWithData();
            var controller = new CarsController(context);

            var result = await controller.GetCarsPlates();

            var actionResult = Assert.IsType<ActionResult<string[]>>(result);
            var plates = Assert.IsType<string[]>(actionResult.Value);
            // "Toyota-AAA123"
            
            var i = context.Cars.FirstOrDefault();
            var plate = $"{i.Model}-{i.Plate}";
            Assert.Contains(plate , plates);
        }
    }
}
