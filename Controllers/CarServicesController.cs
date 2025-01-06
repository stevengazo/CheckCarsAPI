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

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarServicesController : ControllerBase
    {
        private readonly ReportsDbContext _context;

        public CarServicesController(ReportsDbContext context)
        {
            _context = context;
        }

        // GET: api/CarServices
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<CarService>>> GetCarsService()
        {
            return await _context.CarsService.ToListAsync();
        }

        // GET: api/CarServices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CarService>> GetCarService(int id)
        {
            var carService = await _context.CarsService.FindAsync(id);

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

        private bool CarServiceExists(int id)
        {
            return _context.CarsService.Any(e => e.CarServiceId == id);
        }
    }
}
