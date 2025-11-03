using KairosWebAPI.Models.Entities;

namespace KairosWebAPI.Services.DriverLeaveService;

public interface IDriverLeaveService
{
    Task<List<DriverLeave>> GetAllDriverLeavesWithFilters(DateTime? from = null, DateTime? to = null, string? driverName = null, string? leaveType = null);
    Task<DriverLeave> AddLeave(DriverLeave model);
    Task<DriverLeave> UpdateLeave(DriverLeave model);
    Task<DriverLeave> DeleteLeave(DriverLeave model);
}