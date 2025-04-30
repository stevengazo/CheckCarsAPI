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
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using CheckCarsAPI.Services;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrashReportsController : ControllerBase
    {
        private readonly ReportsDbContext _context;
        private readonly EmailService _emailService;

        public CrashReportsController(ReportsDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        #region  EndPoints

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CrashReport>>> GetSearchCrashReport(
             DateTime? date = null,
            string plate = "",
            int carId = 0,
            string author = "")
        {
            try
            {
                // Validación básica de entrada
                if (date is null && string.IsNullOrEmpty(plate) && carId <= 0 && string.IsNullOrEmpty(author))
                {
                    return BadRequest("At least one search parameter must be provided.");
                }
                var query = _context.CrashReports.AsQueryable();
                if (date != null)
                {
                    query = query.Where(e => e.Created.Date == date.Value.Date);
                }

                if (!string.IsNullOrEmpty(plate))
                {
                    query = query.Where(e => e.CarPlate == plate);
                }
                if (carId > 0)
                {
                    query = query.Where(e => e.CarId == carId);
                }
                if (!string.IsNullOrEmpty(author))
                {
                    query = query.Where(e => e.Author == author);
                }

                // Ordenar por fecha descendente y cargar relaciones
                var data = await query
                    .OrderByDescending(e => e.Created)
                    .Take(200)
                    .ToListAsync();

                if (data == null || !data.Any())
                {
                    return NotFound("No matching records found.");
                }

                return Ok(data);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // GET: api/CrashReports
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CrashReport>>> GetCrashReports()
        {
            return await _context.CrashReports
                .OrderByDescending(e=>e.Created)
                .Take(200)
                .ToListAsync();
        }

        // GET: api/CrashReports/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CrashReport>> GetCrashReport(string id)
        {
            var crashReport = await _context.CrashReports.FindAsync(id);

            if (crashReport == null)
            {
                return NotFound();
            }

            return crashReport;
        }

        // PUT: api/CrashReports/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCrashReport(string id, CrashReport crashReport)
        {
            if (!id.Equals(crashReport.ReportId))
            {
                return BadRequest();
            }

            _context.Entry(crashReport).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CrashReportExistsAsync(id))
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

        // POST: api/CrashReports/json
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("json")]
        public async Task<ActionResult<CrashReport>> PostCrashReport(CrashReport crashReport)
        {
            if (await CrashReportExistsAsync(crashReport.ReportId))
            {
                return Conflict("The Report Already Exists");
            }
            _context.CrashReports.Add(crashReport);
            await _context.SaveChangesAsync();
            //            _emailService.SendAlert($"Nuevo Reporte Accidente {crashReport.CarPlate}-{crashReport.Created.ToString("dd-mm-yy")}", $"Se ha generado un nuevo reporte de accidente con el vehiculo {crashReport.CarPlate}.Detalles{crashReport.CrashDetails}");
            await CheckCarDependency(crashReport);

            return CreatedAtAction("GetCrashReport", new { id = crashReport.ReportId }, crashReport);
        }
        // POST: api/CrashReports/form
        [HttpPost("form")]
        public async Task<ActionResult> PostCrashReportForm([FromForm] IFormCollection formData)
        {
            try
            {
                var options = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                var imgFiles = formData.Files.Where(e => e.ContentType.Contains("image")).ToList();
                var ObjectType = nameof(CheckCarsAPI.Models.CrashReport).Split('.').Last();
                var CrashReport = JsonConvert.DeserializeObject<CrashReport>(formData[ObjectType], options);

                if (CrashReport != null && await CrashReportExistsAsync(CrashReport.ReportId))
                {
                    return Conflict("The report already exists");
                }

                List<Photo> photos = CrashReport.Photos.ToList();

                _context.CrashReports.Add(CrashReport);
                await _context.SaveChangesAsync();
                //_emailService.SendAlert($"Nuevo Reporte Accidente {crashReport.CarPlate}-{crashReport.Created.ToString("dd-mm-yy")}", $"Se ha generado un nuevo reporte de accidente con el vehiculo {crashReport.CarPlate}.Detalles{crashReport.CrashDetails}");
                await CheckCarDependency(CrashReport);
                await SaveImagesAsync(imgFiles, CrashReport.ReportId, photos);

                return Created("", CrashReport.ReportId);
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

        // DELETE: api/CrashReports/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCrashReport(string id)
        {
            var crashReport = await _context.CrashReports.FirstAsync(e=>e.ReportId == id);
            if (crashReport == null)
            {
                return NotFound();
            }

            _context.CrashReports.Remove(crashReport);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
        #region  Private Methods
        private async Task CheckCarDependency(CrashReport report)
        {
            var HaveDepency = _context.Cars.Any(e => e.Plate == report.CarPlate);
            if (HaveDepency)
            {
                report.CarId = _context.Cars.FirstOrDefault(e => e.Plate == report.CarPlate).CarId;
                _context.CrashReports.Update(report);
                _context.SaveChanges();
            }
        }
        private async Task<bool> CrashReportExistsAsync(string id)
        {
            return _context.CrashReports.Any(e => e.ReportId.Equals(id));
        }
        private async Task SaveImagesAsync(List<IFormFile> files, string reportId = null, List<Photo> photos = null)
        {
            try
            {
                /// Get the Path of The images folder
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "crashes", reportId);
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }
                foreach (var file in files)
                {
                    // Get the Photo with the same fileName and delete the photo from the list
                    var photo = photos.FirstOrDefault(p => p.FileName == file.FileName);
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
                _context.Photos.UpdateRange(photos);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception e)
            {

                Console.WriteLine(e.Message);
                //throw;
            }
        }
        #endregion
    }
}
