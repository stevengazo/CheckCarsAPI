using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using CheckCarsAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,User,Guest")]
    public class CarsController : ControllerBase
    {
        #region Private Fields
        
        private readonly ReportsDbContext _context;
        private readonly IFileService _fileService;

        #endregion

        #region Constructors

        public CarsController(ReportsDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        #endregion

        #region Public Methods

        // GET: api/Cars
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> GetCars()
        {
            return await _context.Cars.Where(ee=> !ee.Deleted).ToListAsync();
        }


        [HttpGet]
        [Route("available/{id}")]
        public async Task<ActionResult> GetAvailableCars(string id)
        {
            var car = await _context.Cars
                .Where(e => e.CarId == id && e.IsAvailable && !e.Deleted)
                .FirstOrDefaultAsync();
            if (car == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(car); 
            }
        }

        // GET: api/Cars/Plates
        [HttpGet("plates")]
        public async Task<ActionResult<string[]>> GetCarsPlates()
        {
            return await _context.Cars
                .Where(ee => ee.IsAvailable && !ee.Deleted)
                .Select( e=> e.Model + "-" + e.Plate)
                .ToArrayAsync();
        }

        // GET: api/Cars/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Car>> GetCar(string id)
        {
            var car =  _context.Cars.FirstOrDefault(d=> d.CarId == id);

            if (car == null)
            {
                return NotFound();
            }

            return car;
        }

        // PUT: api/Cars/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCar(string id, Car car)
        {
            Console.WriteLine($"available: {car.IsAvailable}");
            if (id != car.CarId)
            {
                return BadRequest();
            }

            _context.Cars.Update(car);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarExists(id))
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

        // POST: api/Cars
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Car>> PostCar(Car car)
        {
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCar", new { id = car.CarId }, car);
        }

        // POST: api/Cars/{id}/image
        // Sube/reemplaza la imagen del vehículo. Devuelve la ruta relativa servida bajo /images.
        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadCarImage(string id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No se recibió ningún archivo.");
            }

            var car = await _context.Cars.FirstOrDefaultAsync(c => c.CarId == id);
            if (car == null)
            {
                return NotFound();
            }

            // Borra la imagen anterior si existía.
            if (!string.IsNullOrWhiteSpace(car.ImagePath))
            {
                _fileService.DeleteFile(car.ImagePath);
            }

            // Guarda en images/cars/<guid>.<ext> y devuelve "cars/<guid>.<ext>".
            var relativePath = await _fileService.SaveFileAsync(file, "cars");
            car.ImagePath = relativePath.Replace("\\", "/");
            _context.Cars.Update(car);
            await _context.SaveChangesAsync();

            return Ok(new { imagePath = car.ImagePath });
        }

        // DELETE: api/Cars/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteCar(string id)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(e => e.CarId == id);

            if (car == null)
            {
                return NotFound();
            }

            car.IsAvailable = false;
            car.Deleted = true;
            _context.Cars.Update(car);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion

        #region Private Methods

        private bool CarExists(string id)
        {
            return _context.Cars.Any(e => e.CarId == id);
        }

        #endregion
    }
}
