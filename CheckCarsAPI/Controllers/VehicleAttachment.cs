using CheckCarsAPI.Data;
using CheckCarsAPI.Models;
using CheckCarsAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheckCarsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleAttachmentsController : ControllerBase
    {
        private readonly ReportsDbContext _context;
        private readonly IFileService _fileService;

        public VehicleAttachmentsController(ReportsDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleAttachment>>> GetAttachments()
        {
            return await _context.VehicleAttachments.Include(v => v.Car).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleAttachment>> GetAttachment(string id)
        {
            var attachment = await _context.VehicleAttachments.Include(v => v.Car).FirstOrDefaultAsync(a => a.AttachmentId == id);

            if (attachment == null)
                return NotFound();

            return attachment;
        }

        [HttpPost("upload/{carId}")]
        public async Task<IActionResult> UploadAttachment(int carId, IFormFile file)
        {
            var carObj = await _context.Cars.FindAsync(carId);
            if (file == null || file.Length == 0)
                return BadRequest("File not selected");

            var filePath = await _fileService.SaveFileAsync(file, carObj.Plate.ToString());

            var attachment = new VehicleAttachment
            {
                FileName = file.FileName,
                FilePath = filePath,
                CarId = carId
            };

            _context.VehicleAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAttachment), new { id = attachment.AttachmentId }, attachment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttachment(string id)
        {
            var attachment = await _context.VehicleAttachments.FindAsync(id);

            if (attachment == null)
                return NotFound();

            var isDeleted = _fileService.DeleteFile(attachment.FilePath);

            if (!isDeleted)
                return StatusCode(StatusCodes.Status500InternalServerError, "File deletion failed");

            _context.VehicleAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
