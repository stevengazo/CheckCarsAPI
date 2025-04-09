using Microsoft.AspNetCore.Http;

namespace CheckCarsAPI.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        bool DeleteFile(string filePath);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var uploadsFolderPath = Path.Combine(_env.WebRootPath, folder);

            if (!Directory.Exists(uploadsFolderPath))
                Directory.CreateDirectory(uploadsFolderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return Path.Combine(folder, fileName);
        }

        public bool DeleteFile(string filePath)
        {
            var fullPath = Path.Combine(_env.WebRootPath, filePath);

            if (!File.Exists(fullPath))
                return false;

            File.Delete(fullPath);

            return true;
        }
    }
}
