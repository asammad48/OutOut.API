using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OutOut.Infrastructure.Services
{
    public class FileUploaderService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileUploaderService> _logger;

        public FileUploaderService(IWebHostEnvironment webHostEnvironment, ILogger<FileUploaderService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public string GetFilePath(string directoryName, string fileName)
        {
            return _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar + directoryName + Path.DirectorySeparatorChar + fileName;
        }

        public async Task<string> UploadFile(string directoryName, byte[] byteArray, string extension)
        {
            if (byteArray == null) return null;

            if (!Directory.Exists(_webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar + directoryName + Path.DirectorySeparatorChar))
            {
                Directory.CreateDirectory(_webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar + directoryName + Path.DirectorySeparatorChar);
            }

            string fileName = Guid.NewGuid().ToString() + extension;
            string path = GetFilePath(directoryName, fileName);

            try
            {
                await File.WriteAllBytesAsync(path, byteArray);
                return fileName;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception while uploading file to dir {directoryName} : {e.Message}");
                return null;
            }
        }

        public async Task<string> UploadFile(string directoryName, IFormFile file)
        {
            if (!Directory.Exists(_webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar + directoryName + Path.DirectorySeparatorChar))
            {
                Directory.CreateDirectory(_webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar + directoryName + Path.DirectorySeparatorChar);
            }
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = GetFilePath(directoryName, fileName);

            try
            {
                using var fileStream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(fileStream);
                return fileName;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception while uploading file to dir {directoryName} : {e.Message}");
                return null;
            }
        }

        public bool DeleteFile(string directoryName, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var path = GetFilePath(directoryName, fileName);
            var dir = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar + directoryName + Path.DirectorySeparatorChar;
            string[] files = Directory.GetFiles(dir);

            if (files.Length < 1 && Array.Exists(files, e => e == path))
                return false;
            else
            {
                try
                {
                    File.Delete(path);
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception while deleting file {path} : {e.Message}");
                    return false;
                }
            }
        }
    }
}
