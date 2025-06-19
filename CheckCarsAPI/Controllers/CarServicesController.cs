using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CheckCarsAPI.Services;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    [Authorize(Roles = "Admin,Manager,User,Guest")]

    public class CarServicesController : ControllerBase
    {
        #region Properties
        
        private readonly ReportsDbContext _context;

        #endregion

        #region Constructor

        public CarServicesController(ReportsDbContext context )
        {
            _context = context;
        }

        #endregion

        #region Endpoints
        // GET: API/CarServies/Resumen
        [HttpGet("Resume")]
        public async Task<IActionResult> Resume()
        {
            var resumen = await _context.Cars
                .Select(car => new CarResumenDto
                {
                    CarId = car.CarId,
                    Brand = car.Brand,
                    Model = car.Model,
                    Plate = car.Plate,

                    ServiciosRecientes = car.Services
                        .Where(s => s.Date.Year == 2025)
                        .Take(100)
                        .OrderByDescending(s => s.Date)
                        .Select(s => new CarServiceDto
                        {
                            CarServiceId = s.CarServiceId,
                            Date = s.Date,
                            Description = s.Description,
                            Mileage = s.mileage,
                            NextMileage = s.NextMileage,
                            Title = s.Title,
                            Type = s.Type
                        }).ToList(),

                    UltimoRetorno = car.VehicleReturns
                        .OrderByDescending(v => v.Created)
                        .Select(v => new VehicleReturnDto
                        {
                            ReportId = v.ReportId,
                            Mileage = v.mileage,
                            Created = v.Created,
                            Author = v.Author
                        }).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(resumen);
        }



        // GET: api/CarServices
        /*  [HttpGet()]
        public async Task<ActionResult<IEnumerable<CarService>>> GetCarsService()
        {
            return await _context.CarsService.ToListAsync();
        }
        */

        // GET: api/CarServices
        [HttpGet("ByCar/{id}")]
        public async Task<ActionResult<IEnumerable<CarService>>> GetCarsService(string id)
        {
            return await _context.CarsService
                .Include(e=> e.Car)
                .Where(e => e.CarId == id)   
                .ToListAsync();
        }

        // GET: api/CarServices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CarService>> GetCarService(string id)
        {
            var carService = await _context.CarsService.FirstAsync(e=>e.CarId == id);

            if (carService == null)
            {
                return NotFound();
            }

            return carService;
        }

        // PUT: api/CarServices/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCarService(int id, CarService carService)
        {
            if (id != carService.CarServiceId)
            {
                return BadRequest();
            }

            _context.Entry(carService).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarServiceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CarServices
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CarService>> PostCarService(CarService carService)
        {
            _context.CarsService.Add(carService);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCarService", new { id = carService.CarServiceId }, carService);
        }

        // DELETE: api/CarServices/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteCarService(int id)
        {
            var carService = await _context.CarsService.FindAsync(id);
            if (carService == null)
            {
                return NotFound();
            }

            _context.CarsService.Remove(carService);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion

        #region Private Methods
        private bool CarServiceExists(int id)
        {
            return _context.CarsService.Any(e => e.CarServiceId == id);
        }

        #endregion


        #region Class DTO

        public class CarResumenDto
        {
            public string CarId { get; set; }
            public string Brand { get; set; }
            public string Model { get; set; }
            public string Plate { get; set; }
            public List<CarServiceDto> ServiciosRecientes { get; set; }
            public VehicleReturnDto UltimoRetorno { get; set; }
        }

        public class CarServiceDto
        {
            public int CarServiceId { get; set; }
            public DateTime Date { get; set; }
            public string Description { get; set; }
            public double Mileage { get; set; }
            public double? NextMileage { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
        }

        public class VehicleReturnDto
        {
            public string ReportId { get; set; }
            public double Mileage { get; set; }
            public DateTime Created { get; set; }
            public string Author { get; set; }
        }

        #endregion
    }
}
