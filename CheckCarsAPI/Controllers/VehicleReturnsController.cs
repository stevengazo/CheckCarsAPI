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
using Microsoft.AspNetCore.Http.HttpResults;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,User,Guest")]
    public class VehicleReturnsController : ControllerBase
    {
        #region private fields

        private readonly ReportsDbContext _context;

        #endregion

        #region Constructors
        public VehicleReturnsController(ReportsDbContext context)
        {
            _context = context;
        }
        #endregion

        #region Public Methods

        // GET: api/VehicleReturns
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<VehicleReturn>>> GetVehicleReturns()
        {
            return await _context.VehicleReturns.ToListAsync();
        }
        // GET: api/VehicleReturns/byCar/id
        [HttpGet("api/VehicleReturns/byCar/{id}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<VehicleReturn>>> GetVehicleReturns(string id)
        {
            try
            {
                int _id = Convert.ToInt32(id);
                var vehicleReturns = await _context.VehicleReturns
                    .Where(e => e.CarId == _id)
                    .ToListAsync();
                return vehicleReturns;
            }
            catch (Exception ef)
            {
                return BadRequest(ef.Message);
            }
        }

        // GET: api/VehicleReturns/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<VehicleReturn>> GetVehicleReturn(string id)
        {
            var vehicleReturn = await _context.VehicleReturns.FindAsync(id);

            if (vehicleReturn == null)
            {
                return NotFound();
            }

            return vehicleReturn;
        }

        // PUT: api/VehicleReturns/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutVehicleReturn(string id, VehicleReturn vehicleReturn)
        {
            if (id != vehicleReturn.ReportId)
            {
                return BadRequest();
            }

            _context.Entry(vehicleReturn).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleReturnExists(id))
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

        // POST: api/VehicleReturns
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<VehicleReturn>> PostVehicleReturn(VehicleReturn vehicleReturn)
        {
            try
            {
                var exist = VehicleReturnExists(vehicleReturn.ReportId);
                // Check if the report already exists
                if (exist)
                {
                    return Conflict("The Report with the Id " + vehicleReturn.ReportId + "exists ");
                }
                vehicleReturn = await CheckCarDependency(vehicleReturn);

                _context.VehicleReturns.Add(vehicleReturn);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetVehicleReturn", new { id = vehicleReturn.ReportId }, vehicleReturn);

            }
            catch (DbUpdateException ef)
            {
                if (VehicleReturnExists(vehicleReturn.ReportId))
                {
                    return Conflict();
                }
                else
                {
                    return Problem("An error occurred while saving the report: " + ef.Message);
                }
            }
        }

        // DELETE: api/VehicleReturns/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteVehicleReturn(string id)
        {
            var vehicleReturn = await _context.VehicleReturns.FindAsync(id);
            if (vehicleReturn == null)
            {
                return NotFound();
            }

            _context.VehicleReturns.Remove(vehicleReturn);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion

        #region Private Methods

        private bool VehicleReturnExists(string id)
        {
            return _context.VehicleReturns.Any(e => e.ReportId == id);
        }

        private async Task<VehicleReturn> CheckCarDependency(VehicleReturn report)
        {
            if (report == null || string.IsNullOrEmpty(report.CarPlate))
            {
                throw new ArgumentNullException(nameof(report), "Report or CarPlate cannot be null or empty.");
            }

            string normalizedPlate = report.CarPlate.ToLower();

            var car = await _context.Cars
                .FirstOrDefaultAsync(e => e.Plate.ToLower() == normalizedPlate);

            if (car != null)
            {
                report.CarId = car.CarId;
                return report;
            }
            return report;
        }
        #endregion

    }
}
