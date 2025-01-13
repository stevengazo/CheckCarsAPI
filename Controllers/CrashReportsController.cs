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

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrashReportsController : ControllerBase
    {
        private readonly ReportsDbContext _context;

        public CrashReportsController(ReportsDbContext context)
        {
            _context = context;
        }

        // GET: api/CrashReports
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CrashReport>>> GetCrashReports()
        {
            return await _context.CrashReports.ToListAsync();
        }

        // GET: api/CrashReports/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CrashReport>> GetCrashReport(int id)
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

        // POST: api/CrashReports
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
            return CreatedAtAction("GetCrashReport", new { id = crashReport.ReportId }, crashReport);
        }

        [HttpPost("form")]
        public async Task<ActionResult> PostCrashReportForm([FromForm] IFormCollection formData)
        {
            try
            {
                var OptionsJson = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                var imgFiles = formData.Files.Where(e => e.ContentType.Contains("image")).ToList();

                CrashReport? crash = JsonConvert.DeserializeObject<CrashReport>(formData[nameof(CrashReport)], OptionsJson);
                Console.WriteLine(crash);
                if (crash == null)
                {
                    return NotFound("Crash object is null");
                }

                if (await CrashReportExistsAsync(crash.ReportId))
                {
                    return Conflict("The Report Already Exists");
                }

                _context.CrashReports.Add(crash);
                _context.SaveChangesAsync();

                List<Photo> photos = crash.Photos.ToList();
              
                SaveImagesAsync(imgFiles, crash.ReportId, crash.Photos.ToList());

                return Created("", crash.ReportId);

            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("The report is null");
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }


        // DELETE: api/CrashReports/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCrashReport(int id)
        {
            var crashReport = await _context.CrashReports.FindAsync(id);
            if (crashReport == null)
            {
                return NotFound();
            }

            _context.CrashReports.Remove(crashReport);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #region  Private Methods

        private async Task<bool> CrashReportExistsAsync(string id)
        {
            return _context.CrashReports.Any(e => e.ReportId.Equals(id));
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
        // Obtén la ruta de la carpeta de imágenes
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "images", "crashes", reportId);
        Directory.CreateDirectory(basePath);

        var updatedPhotos = new List<Photo>(); // Lista para almacenar las fotos actualizadas

        foreach (var file in files)
        {
            // Encuentra la foto correspondiente en la lista
            var photo = photos.FirstOrDefault(p => p.FileName == file.FileName);
            if (photo != null) // Solo continúa si se encuentra la foto
            {
                photos.Remove(photo); // Elimina la foto de la lista original

                // Guardar la imagen en el sistema de archivos y actualizar el objeto de la foto
                if (file.Length > 0)
                {
                    photo.FilePath = Path.Combine(basePath, file.FileName);
                    using (var stream = new FileStream(photo.FilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream); // Copiar el archivo al destino
                    }

                    // Añadir la foto actualizada a la lista de fotos procesadas
                    updatedPhotos.Add(photo);
                }
            }
        }

        // Actualizar las fotos en la base de datos con las fotos procesadas
        if (updatedPhotos.Any())
        {
            _context.Photos.UpdateRange(updatedPhotos);
            await _context.SaveChangesAsync();
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message); // Manejo de excepciones
        // Opcionalmente: lanzar o registrar la excepción
    }
}


        #endregion
    }
}
