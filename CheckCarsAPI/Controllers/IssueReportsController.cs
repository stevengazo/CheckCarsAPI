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
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,User,Guest")]
    public class IssueReportsController : ControllerBase
    {
        #region Properties

        private readonly ReportsDbContext _context;

        #endregion

        #region Constructor

        public IssueReportsController(ReportsDbContext context)
        {
            _context = context;
        }

        #endregion

        #region  EndPoints

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<IssueReport>>> GetSearchIssueReports(
          DateTime? date = null,
          string plate = null,
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

                // Consulta filtrada según los parámetros proporcionados
                var query = _context.IssueReports.AsQueryable();
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
            catch (Exception ex)
            {
                // Registrar errores si se utiliza algún servicio de registro
                return BadRequest($"An error occurred while processing the request: {ex.Message}");
            }
        }


        // GET: api/IssueReports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IssueReport>>> GetIssueReports()
        {
            return await _context.IssueReports.OrderByDescending(e => e.Created).Take(200).ToListAsync();
        }

        // GET: api/IssueReports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IssueReport>> GetIssueReport(string id)
        {
            var issueReport = await _context.IssueReports.FindAsync(id);

            if (issueReport == null)
            {
                return NotFound();
            }

            return issueReport;
        }

        // PUT: api/IssueReports/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIssueReport(string id, IssueReport issueReport)
        {
            if (!id.Equals(issueReport.ReportId))
            {
                return BadRequest();
            }

            try
            {
                _context.IssueReports.Update(issueReport);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await IssueReportExists(id))
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

        // POST: api/IssueReports
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("json")]
        public async Task<ActionResult<IssueReport>> PostIssueReport(IssueReport issueReport)
        {
            if (await IssueReportExists(issueReport.ReportId))
            {
                return Conflict("The Report Already Exists");
            }
            else
            {
                _context.IssueReports.Add(issueReport);
                await _context.SaveChangesAsync();
                await CheckCarDependency(issueReport);
                return Ok();
            }
        }

        [HttpPost("form")]
        public async Task<ActionResult> PostIssueReportForm([FromForm] IFormCollection formData)
        {
            try
            {
                var options = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                var imgFiles = formData.Files.Where(f => f.ContentType.Contains("image")).ToList();
                var issueReport = JsonConvert.DeserializeObject<IssueReport>(formData[nameof(IssueReport)], options);

                if (issueReport == null)
                {
                    return NotFound("The report is null");
                }
                if (await IssueReportExists(issueReport.ReportId))
                {
                    return Conflict("The Report Already Exists");
                }

                List<Photo> photosByReport = issueReport != null ? issueReport.Photos.ToList() : new List<Photo>();
                _context.IssueReports.Add(issueReport);
                await _context.SaveChangesAsync();
                await CheckCarDependency(issueReport);
                await SaveImagesAsync(imgFiles, issueReport.ReportId, photosByReport);

                return Created("", issueReport.ReportId);
            }
            catch (NullReferenceException e)
            {
                return BadRequest("The report is null");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        // DELETE: api/IssueReports/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteIssueReport(string id)
        {
            var issueReport = await _context.IssueReports.FindAsync(id);
            if (issueReport == null)
            {
                return NotFound();
            }

            _context.IssueReports.Remove(issueReport);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion

        #region  Private methods
        /// <summary>
        /// Check if the report have some relation with the cars in the database. If exists, update the report with the CardId
        /// </summary>
        /// <param name="report">Report to check</param>
        /// <returns> A task that represents the asynchronous operation.</returns>
        private async Task CheckCarDependency(IssueReport report)
        {
            var HaveDepency = _context.Cars.Any(e => e.Plate == report.CarPlate);
            if (HaveDepency)
            {
                report.CarId = _context.Cars.FirstOrDefault(e => e.Plate == report.CarPlate).CarId;
                _context.IssueReports.Update(report);
                _context.SaveChanges();
            }
        }
        private async Task<bool> IssueReportExists(string id)
        {
            return _context.IssueReports.Any(e => e.ReportId.Equals(id));
        }
        /// <summary>
        /// Saves a list of images asynchronously to a specified directory and updates the photo records in the database.
        /// </summary>
        /// <param name="files">The list of image files to be saved.</param>
        /// <param name="reportId">The report ID used to create a subdirectory for the images. Default is null.</param>
        /// <param name="photos">The list of photo objects to be updated with the file paths. Default is null.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        /// <exception cref="System.Exception">Thrown when an error occurs during the save operation.</exception>
        private async Task SaveImagesAsync(List<IFormFile> files, string reportId = null, List<Photo> photos = null)
        {
            try
            {
                /// Get the Path of The images folder
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "images", "issues", reportId);
                Directory.CreateDirectory(basePath);

                foreach (var file in files)
                {
                    var photo = photos.FirstOrDefault(p => p.FileName == file.FileName);
                    Console.WriteLine(file.ToString());
                    Console.WriteLine(photo.ToString());
                    if (photo != null && file.Length > 0)
                    {
                        photos.Remove(photo);
                        var newFullPathFile = Path.Combine(basePath, file.FileName);
                        Console.WriteLine("new path:" + newFullPathFile);
                        photo.FilePath = newFullPathFile;
                        using (var stream = new FileStream(newFullPathFile, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        // add the photo updated to the list
                        photos.Add(photo);
                    }
                }

                // Update the photos in the database
                _context.Photos.UpdateRange(photos);
                _context.SaveChanges();
            }
            catch (System.Exception e)
            {

                Console.WriteLine(e.Message);

            }
        }

        private async Task deleteFiles(string path){
            
        }

        #endregion
    }
}
