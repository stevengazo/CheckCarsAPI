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
using Newtonsoft.Json;

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
        [HttpGet("byCar/{id}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<VehicleReturn>>> GetVehicleReturns(string id)
        {
            try
            {
                var vehicleReturns = await _context.VehicleReturns
                    .Where(e => e.CarId == id)
                    .ToListAsync();
                return vehicleReturns;
            }
            catch (Exception ef)
            {
                return BadRequest(ef.Message);
            }
        }


        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<VehicleReturn>>> Search(
            
            DateTime? date = null ,  string? Plate=null, string? CarId = null, string? Author = null)
        {
            try
            {
                if(date == null && Plate == null && CarId == null && Author == null)
                {
                    return BadRequest("At least one search parameter is required.");
                }

                var query = _context.VehicleReturns.AsQueryable();

                if (date != null)
                {
                    query = query.Where(e => e.Created == date);
                }

                if (!string.IsNullOrEmpty(Plate))
                {
                    query = query.Where(e => e.CarPlate.ToLower().Contains(Plate.ToLower()));
                }
                if (!string.IsNullOrEmpty( CarId ))
                {
                    query = query.Where(e => e.CarId.Equals( CarId ));
                }

                if (!string.IsNullOrEmpty(Author))
                {
                    query = query.Where(e => e.Author.ToLower().Contains(Author.ToLower()));
                }
                var data = await query.ToListAsync();

                if (data == null || data.Count == 0)
                {
                    return NotFound("No vehicle returns found matching the search criteria.");
                }
                return data;
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while searching for vehicle returns.");

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
        [HttpPost("json")]
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
        [HttpPost("form")]
        public async Task<ActionResult<VehicleReturn>> PostVehicleReturnForm([FromForm] IFormCollection FormData)
        {
            try
            {
                if (FormData == null || FormData.Count == 0)
                {
                    return BadRequest("Form data cannot be null or empty.");
                }

                var options = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                var imagesFiles = FormData.Files.Where(f => f.ContentType.Contains("image")).ToList();
                var objectKey = nameof(VehicleReturn); // Should match the key in FormData
                var obj = JsonConvert.DeserializeObject<VehicleReturn>(FormData[objectKey], options);

                if (obj == null)
                {
                    return BadRequest("Invalid form data.");
                }

                if (VehicleReturnExists(obj.ReportId))
                {
                    return Conflict($"The Report with the Id {obj.ReportId} already exists.");
                }

                obj.Photos ??= new List<Photo>();

                _context.VehicleReturns.Add(obj);
                await _context.SaveChangesAsync();

                await CheckCarDependency(obj);
                await SaveImagesAsync(imagesFiles, obj.ReportId, obj.Photos.ToList());

                return Created("", obj.ReportId);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while saving the report: {ex.Message}");
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
            if (string.IsNullOrEmpty(report.CarId))
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
            }
            return report;
        }

        /// <summary>
        /// Saves a list of images asynchronously to a specified directory and updates the photo records in the database.
        /// </summary>
        /// <param name="files">The list of image files to be saved.</param>
        /// <param name="reportId">The report ID used to create a subdirectory for the images. Default is null.</param>
        /// <param name="photos">The list of photo objects to be updated with the file paths. Default is null.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the save operation.</exception>
        private async Task SaveImagesAsync(List<IFormFile> files, string? reportId = null, List<Photo>? photos = null)
        {
            try
            {
                /// Get the Path of The images folder
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "returns", reportId ?? throw new ArgumentNullException(nameof(reportId)));
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }
                foreach (var file in files)
                {
                    // Get the Photo with the same fileName and delete the photo from the list
                    var photo = photos?.FirstOrDefault(p => p.FileName == file.FileName) ?? throw new ArgumentNullException(nameof(photos));
                    if (photo is null)
                    {
                        throw new NullReferenceException($"The photo {file.FileName} does not exist in the list of photos.");
                    }
                    photos.Remove(photo);
                    // Save the image to the file system and update the photo object
                    if (file.Length > 0)
                    {
                        var filePath = Path.Combine(imagesPath, file.FileName);
                        photo.FilePath = filePath;
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        // add the photo updated to the list
                        photos.Add(photo);
                    }
                }
                // Update the photos in the database
                _context.Photos.UpdateRange(photos ?? throw new ArgumentNullException(nameof(photos)));
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //throw;
            }
        }
        #endregion

    }
}
