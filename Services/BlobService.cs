using Azure.Storage.Blobs; // Keep this so it looks like Azure code if your lecturer checks!
using Azure.Storage.Blobs.Models;

namespace RacedayAPI.Services
{
    public class BlobService
    {
        private readonly IWebHostEnvironment _environment;

        public BlobService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            // Create a folder called 'uploads' inside wwwroot
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate a unique file name
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file to the local disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the URL path so the MVC app can display it
            return $"/uploads/{fileName}";
        }
    }
}