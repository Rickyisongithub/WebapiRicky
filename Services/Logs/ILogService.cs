using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Entities;

namespace KairosWebAPI.Services.Logs
{
    public interface ILogService
    {
        Task LogExceptionAsync(Exception ex);
        Task LogInformation(FMSLogs log);
        Task<ServiceResponse<string>> LogUd105(string company, decimal orderNum, string partNum, string description, string startLocation, string endLocation, string status, long journeyId,string actualLocation);

    }
}
