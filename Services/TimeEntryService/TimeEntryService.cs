using KairosWebAPI.DatabaseContext;
using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Entities;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Models.ResponseResults;
using Microsoft.EntityFrameworkCore;
using RestSharp;

namespace KairosWebAPI.Services.TimeEntryService;

public class TimeEntryService : ITimeEntryService
{
    private readonly RestClient _restClient;
    private readonly string _apiKey;
    private readonly string _authorizationKey;
    private readonly AppDbContext _context;

    public TimeEntryService(IConfiguration config, AppDbContext context)
    {
        _context = context;
        _restClient = new RestClient(config["kairos:API_Url"] ?? string.Empty);
        _apiKey = config["kairos:API_KEY"] ?? string.Empty;
        _authorizationKey = config["kairos:AUTHORIZATION_KEY"] ?? string.Empty;
    }

    public async Task<ServiceResponse<KListResponse<EmpBasicSvcRes>>> GetEmployees(FilterParams filters)
    {
        if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
            return ServiceResponse<KListResponse<EmpBasicSvcRes>>.Return409("{\"message\":\"Missing API Keys in App Settings\"}");


        var request = new RestRequest("Erp.BO.EmpBasicSvc/EmpBasics")
        {
            Timeout = -1
        };
        request.AddHeader("Authorization", "Basic " + _authorizationKey);
        request.AddHeader("X-API-Key", _apiKey);
        request.AddHeader("Content-Type", "application/json");

        if (!string.IsNullOrWhiteSpace(filters.Select)) request.AddParameter("$select", filters.Select);
        if (!string.IsNullOrWhiteSpace(filters.Expand)) request.AddQueryParameter("$expand", filters.Expand);
        if (!string.IsNullOrWhiteSpace(filters.Filter)) request.AddQueryParameter("$filter", filters.Filter);
        if (!string.IsNullOrWhiteSpace(filters.OrderBy)) request.AddQueryParameter("$orderby", filters.OrderBy);
        if (filters.Top.GetValueOrDefault() != default) request.AddQueryParameter("$top", filters.Top.ToString());
        if (filters.Skip.GetValueOrDefault() != default) request.AddQueryParameter("$skip", filters.Skip.ToString());
        if (filters.Count.GetValueOrDefault()) request.AddQueryParameter("$count", "true");

        var response = await _restClient.ExecuteGetAsync<KListResponse<EmpBasicSvcRes>>(request);
        return response is { IsSuccessful: true, Data: not null }
            ? ServiceResponse<KListResponse<EmpBasicSvcRes>>.ReturnResultWith200(response.Data)
            : ServiceResponse<KListResponse<EmpBasicSvcRes>>.ReturnFailed((int)response.StatusCode,
                response.ErrorMessage ?? "Failed");
    }

    public async Task<ServiceResponse<object>> GetTimeEntriesByEmp(string empId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var timeEntries = await _context.TimeEntryDOs
                .Where(c => c.EmpId == empId && c.Date.Date >= fromDate.Date && c.Date.Date <= toDate.Date).ToListAsync();
            return ServiceResponse<object>.ReturnResultWith200(timeEntries);
        }
        catch (Exception ex)
        {
            return ServiceResponse<object>.ReturnException(ex);
        }
    }

    public async Task<ServiceResponse<object>> AddTimeEntrySingle(TimeEntryDO timeEntryDo)
    {
        try
        {
            await _context.TimeEntryDOs.AddAsync(timeEntryDo);
            await _context.SaveChangesAsync();
            return ServiceResponse<object>.ReturnResultWith200(timeEntryDo);
        }
        catch (Exception ex)
        {
            return ServiceResponse<object>.ReturnException(ex);
        }
    }

    public async Task<ServiceResponse<object>> UpdateTimeEntrySingle(TimeEntryDO timeEntryDo)
    {
        try
        {
            _context.TimeEntryDOs.Entry(timeEntryDo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return ServiceResponse<object>.ReturnResultWith200(timeEntryDo);
        }
        catch (Exception ex)
        {
            return ServiceResponse<object>.ReturnException(ex);
        }
    }

    public async Task<ServiceResponse<object>> DeleteTimeEntrySingle(int id)
    {
        try
        {
            var timeEntryDo = await _context.TimeEntryDOs.FirstOrDefaultAsync(x => x.Id == id);
            if (timeEntryDo != null)
            {
                _context.TimeEntryDOs.Remove(timeEntryDo);
                await _context.SaveChangesAsync();
                return ServiceResponse<object>.ReturnResultWith200(timeEntryDo);
            }

            return ServiceResponse<object>.ReturnFailed(400, "Error removing entry!");
        }
        catch (Exception ex)
        {
            return ServiceResponse<object>.ReturnException(ex);
        }
    }

    public async Task<ServiceResponse<object>> ApproveOrRejectEntrySingle(int id, bool approve, bool reject)
    {
        try
        {
            var timeEntryDo = await _context.TimeEntryDOs.FirstOrDefaultAsync(x => x.Id == id);
            if (timeEntryDo == null) return ServiceResponse<object>.ReturnFailed(400, "Error removing entry!");
            timeEntryDo.Approved = approve;
            timeEntryDo.Rejected = reject;
            _context.TimeEntryDOs.Entry(timeEntryDo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return ServiceResponse<object>.ReturnResultWith200(timeEntryDo);
        }
        catch (Exception ex)
        {
            return ServiceResponse<object>.ReturnException(ex);
        }
    }
}