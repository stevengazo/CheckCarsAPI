﻿using System;
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

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CrashReportsController : ControllerBase
    {
        private readonly ReportsDbContext _context;

        public CrashReportsController(ReportsDbContext context)
        {
            _context = context;
        }

        // GET: api/CrashReports
        [HttpGet]

        public async Task<ActionResult<IEnumerable<CrashReport>>> GetCrashReports()
        {
            return await _context.CrashReports.ToListAsync();
        }

        // GET: api/CrashReports/5
        [HttpGet("{id}")]
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
                if (!CrashReportExists(id))
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
            _context.CrashReports.Add(crashReport);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCrashReport", new { id = crashReport.ReportId }, crashReport);
        }

        [HttpPost("form")]
        public async Task<ActionResult> PostCrashReportForm([FromForm] IFormCollection formData)
        {
            try
            {
                var imgFiles = formData.Files.Where(e => e.ContentType.Contains("image")).ToList();
                var options = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                CrashReport crash = JsonConvert.DeserializeObject<CrashReport>(formData["crash"], options);

                if (crash != null && CrashReportExists(crash.ReportId))
                {
                    return BadRequest("The report already exists");
                }

                _context.CrashReports.Add(crash);
                _context.SaveChangesAsync();


                List<Photo> photos = crash.Photos.ToList();
                SaveImagesAsync(imgFiles, crash.ReportId, crash.Photos.ToList());

                return Created("", crash);

            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }


        // DELETE: api/CrashReports/5
        [HttpDelete("{id}")]
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

        private bool CrashReportExists(string id)
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
                /// Get the Path of The images folder
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "crash", reportId);
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
