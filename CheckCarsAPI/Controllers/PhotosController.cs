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

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PhotosController : ControllerBase
    {
        private readonly ReportsDbContext _context;

        public PhotosController(ReportsDbContext context)
        {
            _context = context;
        }

        // GET: api/Photos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos()
        {
            return await _context.Photos.ToListAsync();
        }

        // GET: api/Photos/report/5
        [HttpGet("report/{id}")]
        public async Task<ActionResult<List<Photo>>> GetPhotosByReport(string id)
        {
            var photos = await _context.Photos.Where(e => e.ReportId == id).ToListAsync();

            if (photos == null || !photos.Any())
            {
                return NotFound();
            }

            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "images");
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}".TrimEnd('/');

            foreach (var photo in photos)
            {
                // Asegura que obtienes una ruta relativa correcta multiplataforma
                var relativePath = Path.GetRelativePath(imagesFolder, photo.FilePath);

                // Normaliza separadores de directorio para web
                var normalizedPath = relativePath.Replace(Path.DirectorySeparatorChar, '/').Replace("//", "/");

                // Construye la URL final accesible
                photo.FilePath = $"{baseUrl}/images/{normalizedPath}";
            }

            return photos;
        }


        // GET: api/Photos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Photo>> GetPhoto(string id)
        {
            var photo = await _context.Photos.FindAsync(id);

            if (photo == null)
            {
                return NotFound();
            }

            return photo;
        }

        // PUT: api/Photos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPhoto(string id, Photo photo)
        {
            if (id != photo.PhotoId)
            {
                return BadRequest();
            }


            try
            {
                var local = _context.Photos.Local.FirstOrDefault(entry => entry.PhotoId == photo.PhotoId);
                if (local != null)
                {
                    _context.Entry(local).State = EntityState.Detached;
                }
                _context.Photos.Update(photo);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhotoExists(id))
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

        // POST: api/Photos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Photo>> PostPhoto(Photo photo)
        {
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPhoto", new { id = photo.PhotoId }, photo);
        }

        // DELETE: api/Photos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(string id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(e => e.PhotoId == id);
            if (photo == null)
            {
                return NotFound();
            }

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PhotoExists(string id)
        {
            return _context.Photos.Any(e => e.PhotoId == id);
        }
    }
}
