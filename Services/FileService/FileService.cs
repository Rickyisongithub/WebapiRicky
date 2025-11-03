namespace KairosWebAPI.Services.FileService
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<string> SaveReport(string fileName, string bytesArray, string fileUrl)
        {
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot/Files");
            byte[] bytes = Convert.FromBase64String(bytesArray);
            var filePath = Path.Combine(path, fileName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            await File.WriteAllBytesAsync(filePath, bytes);
            return fileUrl + fileName;
        }
    }
}
