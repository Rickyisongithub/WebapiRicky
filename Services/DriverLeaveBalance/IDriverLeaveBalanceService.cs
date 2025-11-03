using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.Entities;

namespace KairosWebAPI.Services.DriverLeaveBalanceService {

    public interface IDriverLeaveBalanceService
    {
        Task<List<DriverLeaveBalance>> GetAllBalances();
        Task<DriverLeaveBalance?> GetBalanceByDriverAndType(string driverName, string leaveType);
        Task<DriverLeaveBalance> AddOrUpdateBalance(DriverLeaveBalance balance);
        Task<bool> DeductLeave(string driverName, string leaveType);

        Task<(bool success, string message)> RequestLeave(string driverName, string leaveType, int daysRequested);
        Task<List<LeaveBalanceResponseDto>> GetLeaveBalances(DateTime from, DateTime to, List<string> truckIds);
        Task<(bool success, string message)> CheckLeaveBalance(string driverName, string leaveType, int daysRequested);
        Task<List<DriverLeaveBalance>> GetBalanceByDriver(string driverName);
    }
}   