using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Entities;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Models.ResponseResults;

namespace KairosWebAPI.Services.TimeEntryService;

public interface ITimeEntryService
{
    Task<ServiceResponse<KListResponse<EmpBasicSvcRes>>> GetEmployees(FilterParams filters);
    Task<ServiceResponse<object>> GetTimeEntriesByEmp(string empId, DateTime fromDate, DateTime toDate);
    Task<ServiceResponse<object>> AddTimeEntrySingle(TimeEntryDO timeEntryDo);
    Task<ServiceResponse<object>> UpdateTimeEntrySingle(TimeEntryDO timeEntryDo);
    Task<ServiceResponse<object>> DeleteTimeEntrySingle(int id);
    Task<ServiceResponse<object>> ApproveOrRejectEntrySingle(int id, bool approve, bool reject);
}