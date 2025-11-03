using System.Globalization;
using KairosWebAPI.DatabaseContext;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.Entities;
using KairosWebAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KairosWebAPI.Services.DriverLeaveBalanceService
{

    public class DriverLeaveBalanceService : IDriverLeaveBalanceService
    {
        private readonly AppDbContext _context;

        public DriverLeaveBalanceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DriverLeaveBalance>> GetAllBalances()
        {
            return await _context.DriverLeaveBalances.ToListAsync();
        }

        public async Task<DriverLeaveBalance?> GetBalanceByDriverAndType(string driverName, string leaveType)
        {
            return await _context.DriverLeaveBalances
                .FirstOrDefaultAsync(b => b.DriverName == driverName && b.LeaveType == leaveType);
        }

        public async Task<List<DriverLeaveBalance>> GetBalanceByDriver(string driverName)
        {
            return await _context.DriverLeaveBalances
                .Where(b => b.DriverName == driverName)
                .ToListAsync();
        }

        public async Task<DriverLeaveBalance> AddOrUpdateBalance(DriverLeaveBalance balance)
        {
            var existingBalance = await _context.DriverLeaveBalances
                .FirstOrDefaultAsync(b => b.DriverName == balance.DriverName && b.LeaveType == balance.LeaveType);

            if (existingBalance != null)
            {
                existingBalance.TotalLeaves = balance.TotalLeaves;
                existingBalance.UsedLeaves = balance.UsedLeaves;
                _context.Entry(existingBalance).State = EntityState.Modified;
            }
            else
            {
                await _context.DriverLeaveBalances.AddAsync(balance);
            }

            await _context.SaveChangesAsync();
            return balance;
        }

        public async Task<bool> DeductLeave(string driverName, string leaveType)
        {
            var balance = await _context.DriverLeaveBalances
                .FirstOrDefaultAsync(b => b.DriverName == driverName && b.LeaveType == leaveType);

            if (balance == null || balance.UsedLeaves >= balance.TotalLeaves)
                return false; // No leaves left

            balance.UsedLeaves++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool success, string message)> RequestLeave(string driverName, string leaveType, int daysRequested)
        {
            var balance = await _context.DriverLeaveBalances
                .FirstOrDefaultAsync(b => b.DriverName == driverName && b.LeaveType == leaveType);

            if (balance == null)
            {
                throw new KeyNotFoundException("Leave balance not found for the driver.");
            }

            if (balance.UsedLeaves + daysRequested > balance.TotalLeaves)
            {
                throw new InvalidOperationException("Insufficient leave balance.");
            }

            // Deduct leave balance
            balance.UsedLeaves += daysRequested;
            _context.Entry(balance).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return (true, "Leave approved.");
        }

        public async Task<(bool success, string message)> CheckLeaveBalance(string driverName, string leaveType, int daysRequested)
        {
            var balance = await _context.DriverLeaveBalances
                .FirstOrDefaultAsync(b => b.DriverName == driverName && b.LeaveType == leaveType);

            if (balance == null)
            {
                return (false, "Leave balance not found for the driver.");
            }

            if (balance.UsedLeaves + daysRequested > balance.TotalLeaves)
            {
                return (false, "Insufficient leave balance.");
            }

            return (true, "Balance exists.");
        }

        public async Task<List<LeaveBalanceResponseDto>> GetLeaveBalances(DateTime from, DateTime to, List<string> truckIds)
        {
            var balances = await _context.DriverLeaveBalances
                .Where(b => truckIds.Contains(b.DriverName)) // Assuming DriverName represents the truck ID (adjust if needed)
                .ToListAsync();

            var leaveRequests = await _context.DriverLeaves
                .Where(l => truckIds.Contains(l.DriverName) && l.LeaveDate >= from && l.LeaveDate <= to)
                .ToListAsync();


            // ✅ Construct response
            var response = balances.Select(balance => new LeaveBalanceResponseDto
            {
                PartNum = balance.DriverName,
                Description = balance.DriverName, // Modify if truck descriptions are stored elsewhere
                Stats = new LeaveStats
                {
                    Allowed = balance.TotalLeaves,
                    Applied = leaveRequests.Count(l => l.DriverName == balance.DriverName),
                    Pending = leaveRequests.Count(l => l.Approved == ApprovalStatus.Pending),
                    Approved = leaveRequests.Count(l => l.Approved == ApprovalStatus.Approved),
                    Rejected = leaveRequests.Count(l => l.Approved == ApprovalStatus.Rejected)
                },
                Leaves = leaveRequests
                    .Where(l => l.DriverName == balance.DriverName)
                    .Select(l => new LeaveRecordDto
                    {
                        Id = l.Id,
                        DriverName = l.DriverName,
                        LeaveType = l.LeaveType,
                        LeaveReason = l.LeaveReason,
                        LeaveDate = l.LeaveDate.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                        Approved = l.Approved == ApprovalStatus.Approved,
                        ApprovedBy = l.ApprovedBy
                    }).ToList()
            }).ToList();

            return response;
        }
    }
}