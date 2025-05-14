using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CheckCarsAPI.Models;
using CheckCarsAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using NuGet.Protocol;
using CheckCarsAPI.Services;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EntryExitReportsController : ControllerBase
    {
        private readonly ReportsDbContext _context;
        private readonly EmailService _EmailService;

        public EntryExitReportsController(
            ReportsDbContext context,
            EmailService email)
        {
            _context = context;
            _EmailService = email;
        }
        #region  EndPoints
        [HttpGet("ByCar/{id}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<EntryExitReport>>> GetByCar(int carId = 0)
        {    
            try
            {
                var query = _context.EntryExitReports.AsQueryable();
                if (carId == 0)    
                {
                return ValidationProblem("El id del coche no puede ser 0");
                }
                else
                {
                    query = query.Where(e => e.CarId == carId); 
                }


                if (query.Count() == 0)
                {
                    return NotFound("No se encontraron registros que coincidan con los criterios de búsqueda.");
                }

                var data = await query
                    .OrderByDescending(e => e.Created)
                    .Take(200)
                    .ToListAsync();
                if (!data.Any())
                {
                    return NotFound("No se encontraron registros que coincidan con los criterios de búsqueda.");
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                // Idealmente usar un logger en lugar de retornar el error detallado al cliente
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<EntryExitReport>>> GetSearchExitReports(
            DateTime? date = null,
            string? plate = null,
            int? carId = null,
            string? author = null)
        {
            try
            {
                // Validación básica: al menos un parámetro debe estar presente
                if (date is null && string.IsNullOrWhiteSpace(plate) && carId is null && string.IsNullOrWhiteSpace(author))
                {
                    return BadRequest("Debe proporcionar al menos un parámetro de búsqueda.");
                }

                var query = _context.EntryExitReports.AsQueryable();

                if (date.HasValue)
                {
                    var targetDate = date.Value.Date;
                    query = query.Where(e => e.Created.Date == targetDate);
                }

                if (!string.IsNullOrWhiteSpace(plate))
                {
                    query = query.Where(e => e.CarPlate.Contains(plate));
                }

                if (carId.HasValue)
                {
                    query = query.Where(e => e.CarId == carId.Value);
                }

                if (!string.IsNullOrWhiteSpace(author))
                {
                    query = query.Where(e => e.Author.Contains(author) );
                }
                if(query.Count() == 0)
                {
                    return  NotFound("No se encontraron registros que coincidan con los criterios de búsqueda.");
                }

                var data = await query
                    .OrderByDescending(e => e.Created)
                    .Take(200)
                    .ToListAsync();

                if (!data.Any())
                {
                    return NotFound("No se encontraron registros que coincidan con los criterios de búsqueda.");
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                // Idealmente usar un logger en lugar de retornar el error detallado al cliente
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }


        // GET: api/EntryExitReports
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<EntryExitReport>>> GetEntryExitReports()
        {
            return await _context.EntryExitReports
                .Take(200)
                .OrderByDescending(e => e.Created)
                .ToListAsync();
        }

        // GET: api/EntryExitReports/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<EntryExitReport>> GetEntryExitReport(string id)
        {
            var entryExitReport = await _context.EntryExitReports.FindAsync(id);
            if (entryExitReport == null)
            {
                return NotFound();
            }
            return entryExitReport;
        }

        // PUT: api/EntryExitReports/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutEntryExitReport(string id, EntryExitReport entryExitReport)
        {
            if (!id.Equals(entryExitReport.ReportId))
            {
                return BadRequest();
            }
            _context.Entry(entryExitReport).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntryExitReportExists(id))
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

        // POST: api/EntryExitReport
        [HttpPost("json")]
        public async Task<ActionResult> PostEntryExitReport(EntryExitReport entryExitReport)
        {
            try
            {
                var exits = CheckEntryExitReport(entryExitReport.ReportId);
                if (exits.Result)
                {
                    return Conflict("The report already exists");
                }
                _context.EntryExitReports.Add(entryExitReport);
                await _context.SaveChangesAsync();
                await CheckCarDependency(entryExitReport);
                return CreatedAtAction(nameof(GetEntryExitReport), new { id = entryExitReport.ReportId }, entryExitReport);
            }
            catch (SqlException e)
            {
                return BadRequest($"Error processing the request: {e.InnerException?.Message}");
            }
        }

        // POST: api/EntryFullReport
        [HttpPost("form")]
        public async Task<ActionResult> PostEntryExitReport([FromForm] IFormCollection formData)
        {
            try
            {
                if (formData == null || formData.Count == 0)
                {
                    return BadRequest("Form Data is null or empty.");
                }
                else
                {
                    var options = new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                    var imgFiles = formData.Files.Where(e => e.ContentType.Contains("image")).ToList();
                    var ObjectType = nameof(EntryExitReport).Split('.').Last();
                    var entryExits = JsonConvert.DeserializeObject<EntryExitReport>(formData[ObjectType], options);

                    if (entryExits != null && await CheckEntryExitReport(entryExits.ReportId))
                    {
                        return Conflict("The report already exists");
                    }

                    if (entryExits == null)
                    {
                        throw new ArgumentNullException(nameof(entryExits), "EntryExits cannot be null.");
                    }

                    entryExits.Photos ??= new List<Photo>();
                    List<Photo> photos = entryExits.Photos.ToList();

                    _context.EntryExitReports.Add(entryExits);
                    await _context.SaveChangesAsync();
                    await CheckCarDependency(entryExits);

                    await SaveImagesAsync(imgFiles, entryExits.ReportId, photos);

                    return Created("", entryExits.ReportId);
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("The report is null");
            }
            catch (SqlException e)
            {
                return BadRequest($"Error processing the request: {e.InnerException?.Message}");
            }
        }

        // DELETE: api/EntryExitReports/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEntryExitReport(string id)
        {
            var entryExitReport = await _context.EntryExitReports.FindAsync(id);
            if (entryExitReport == null)
            {
                return NotFound();
            }
            _context.EntryExitReports.Remove(entryExitReport);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region  Private Methods

        /// <summary>
        /// Checks if the car associated with the given report has a dependency in the database.
        /// If a dependency is found, updates the report with the corresponding CarId and saves the changes.
        /// </summary>
        /// <param name="report">The entry/exit report to check for car dependency.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task CheckCarDependency(EntryExitReport report)
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
                _context.EntryExitReports.Update(report);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<bool> CheckEntryExitReport(string id)
        {
            return await _context.EntryExitReports.AnyAsync(e => e.ReportId == id);
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
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "entries", reportId ?? throw new ArgumentNullException(nameof(reportId)));
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

        /// <summary>
        /// Check if the report exists in the database.
        /// </summary>
        /// <param name="id">Id Report to check</param>
        /// <returns>true if the report was founded</returns>
        private bool EntryExitReportExists(string id)
        {
            return _context.EntryExitReports.Any(e => e.ReportId.Equals(id));
        }
        #endregion
    }
}
