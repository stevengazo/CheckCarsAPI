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

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IssueReportsController : ControllerBase
    {
        private readonly ReportsDbContext _context;

        public IssueReportsController(ReportsDbContext context)
        {
            _context = context;
        }

        // GET: api/IssueReports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IssueReport>>> GetIssueReports()
        {
            return await _context.IssueReports.ToListAsync();
        }

        // GET: api/IssueReports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IssueReport>> GetIssueReport(int id)
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

            _context.Entry(issueReport).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IssueReportExists(id))
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
            try
            {
                var exits = IssueReportExists(issueReport.ReportId);
                if (exits)
                {
                    return BadRequest("The Report Already Exists");
                }
                _context.IssueReports.Add(issueReport);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetIssueReport", new { id = issueReport.ReportId }, issueReport);
            }
            catch (Exception e)
            {

                throw;
            }
        }

        [HttpPost("form")]
        public async Task<ActionResult> PostIssueReportForm([FromForm] IFormCollection formData)
        {
            try
            {
                var imgFiles = formData.Files.Where(f => f.Name.Contains("image")).ToList();
                JsonSerializerSettings? opt = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                var issue = JsonConvert.DeserializeObject<IssueReport>(formData["issue"], opt);
                if (issue == null && IssueReportExists(issue.ReportId))
                {
                    return BadRequest("The report already exists");
                }
                List<Photo> photos = issue.Photos.ToList();

                _context.IssueReports.Add(issue);
                await _context.SaveChangesAsync();
                SaveImagesAsync(imgFiles, issue.ReportId, photos);
                return Created("", issue);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }


        // DELETE: api/IssueReports/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssueReport(int id)
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

        private bool IssueReportExists(string id)
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
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "issues", reportId);
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


    }
}
