namespace KairosWebAPI.Services.FileService
{
    public interface IFileService
    {
        Task<string> SaveReport(string fileName, string bytesArray, string fileUrl);
    }
}
