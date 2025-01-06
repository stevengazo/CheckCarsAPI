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
using System.Text.Json;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using NuGet.Protocol;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EntryExitReportsController : ControllerBase
    {
        private readonly ReportsDbContext _context;

        public EntryExitReportsController(ReportsDbContext context)
        {
            _context = context;
        }

        // GET: api/EntryExitReports
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<EntryExitReport>>> GetEntryExitReports()
        {
            //  return await _context.EntryExitReports.ToListAsync();
            return Ok(new string[] { "Value1", "Value2" });
        }

        // GET: api/EntryExitReports/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<EntryExitReport>> GetEntryExitReport(int id)
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


        //POST: api/EntryExitReports
        [HttpPost]
        public async Task<ActionResult<string>> PostEntryExitReport([FromForm] IFormCollection formData)
        {
            try
            {
                // Get the images and the report data from the form data
                var ImgFiles = formData.Files.Where(e => e.ContentType.Contains("image")).ToList();
                List<EntryExitReport> entryExits = formData.Select(x => JsonConvert.DeserializeObject<EntryExitReport>(x.Value)).ToList();
                List<Photo> photos = entryExits.SelectMany(e => e.Photos).ToList();
                // Save the report to the database
                _context.EntryExitReports.AddRange(entryExits);
                await _context.SaveChangesAsync();
                // Save the images to the file system
                var ReportId = entryExits[0].ReportId;
                // Save the images to the file system and DB
                await SaveImagesAsync(ImgFiles, ReportId, photos);
                return CreatedAtAction(nameof(GetEntryExitReport), new { id = entryExits[0].ReportId }, entryExits);
            }
            catch (SqlException e)
            {
                return BadRequest($"Error processing the request: {e.InnerException?.Message}");
            }
        }


        // DELETE: api/EntryExitReports/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEntryExitReport(int id)
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

        #region  Private Methods
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
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "entries", reportId);
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
                throw;
            }
        }

        private bool EntryExitReportExists(string id)
        {
            return _context.EntryExitReports.Any(e => e.ReportId.Equals(id));
        }
        #endregion
    }
}
