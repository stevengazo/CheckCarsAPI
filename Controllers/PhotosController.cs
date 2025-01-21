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
            // Obtén las fotos asociadas al informe
            var photos = await _context.Photos.Where(e => e.ReportId == id).ToListAsync();

            // Si no se encuentran fotos, retorna un NotFound
            if (photos == null || !photos.Any())
            {
                return NotFound();
            }

            // Obtener la URL base del servidor
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}".TrimEnd('/');

            // Modificar las URLs de las fotos para que sean accesibles
            foreach (var photo in photos)
            {
                // Aquí se asume que los archivos están siendo servidos desde "/images/"
                // Ajusta según el "RequestPath" configurado en StaticFileOptions
                var relativePath = photo.FilePath
                    .Replace(@"C:\IIIS_WebSite\cars\images\", "") // Remueve la parte de la ruta local
                    .Replace("\\", "/"); // Convierte backslashes a slashes para URLs

                photo.FilePath = $"{baseUrl}/images/{relativePath}";
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

            _context.Entry(photo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null)
            {
                return NotFound();
            }
            System.IO.File.Delete(photo.FilePath);
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
