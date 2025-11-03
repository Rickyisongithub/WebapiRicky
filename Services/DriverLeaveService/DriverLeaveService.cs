using KairosWebAPI.DatabaseContext;
using KairosWebAPI.Models.Entities;
using KairosWebAPI.Models.Enums;
using KairosWebAPI.Services.DriverLeaveBalanceService;
using Microsoft.EntityFrameworkCore;

namespace KairosWebAPI.Services.DriverLeaveService;

public class DriverLeaveService : IDriverLeaveService
{
    private readonly AppDbContext _context;
    private readonly IDriverLeaveBalanceService _balanceService;

    public DriverLeaveService(AppDbContext context, IDriverLeaveBalanceService balanceService)
    {
        _context = context;
        _balanceService = balanceService;
    }

    public async Task<List<DriverLeave>> GetAllDriverLeavesWithFilters(DateTime? from = null, DateTime? to = null, string? driverName = null, string? leaveType = null)
    {
        try
        {
            // Start with the full dataset
            var query = _context.DriverLeaves.AsQueryable();

            // Apply date filters if provided
            if (from.HasValue)
            {
                query = query.Where(c => c.LeaveDate >= from.Value);
            }
            if (to.HasValue)
            {
                query = query.Where(c => c.LeaveDate <= to.Value);
            }

            // Apply driver name filter if provided
            if (!string.IsNullOrEmpty(driverName))
            {
                query = query.Where(c => c.DriverName == driverName);
            }

            // Apply leave type filter if provided
            if (!string.IsNullOrEmpty(leaveType))
            {
                query = query.Where(c => c.LeaveType == leaveType);
            }

            return await query.ToListAsync();
        }
        catch (Exception)
        {
            throw new Exception("An error occurred while fetching driver leaves.");
        }
    }


    public async Task<DriverLeave> AddLeave(DriverLeave model)
    {
        try
        {
            var (balanceSuccess, balanceMessage) = await _balanceService.CheckLeaveBalance(model.DriverName, model.LeaveType, model.Days);

            if (!balanceSuccess)
            {
                throw new Exception(balanceMessage);
            }

            await _context.DriverLeaves.AddAsync(model);
            await _context.SaveChangesAsync();

            if (model.Approved == ApprovalStatus.Approved)
            {
                var (deductSuccess, deductMessage) = await _balanceService.RequestLeave(model.DriverName, model.LeaveType, 1);

                if (!deductSuccess)
                {
                    throw new Exception(deductMessage);
                }
            }

            return model;
        }
        catch (KeyNotFoundException ex)
        {
            throw new KeyNotFoundException($"Leave request failed: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"Leave request failed: {ex.Message}");
        }
        catch (Exception)
        {
            throw new Exception();
        }
    }

    public async Task<DriverLeave> UpdateLeave(DriverLeave model)
    {
        try
        {
            var existingLeave = await _context.DriverLeaves.FindAsync(model.Id);
            if (existingLeave == null)
            {
                throw new Exception("Leave request not found.");
            }

            if (existingLeave.Approved == ApprovalStatus.Approved)
            {
                throw new Exception("Leave has already been approved. No further updates allowed.");
            }

            existingLeave.Approved = model.Approved;
            existingLeave.ApprovedBy = model.ApprovedBy;

            if (model.Approved == ApprovalStatus.Approved)
            {
                var (deductSuccess, deductMessage) = await _balanceService.RequestLeave(existingLeave.DriverName, existingLeave.LeaveType, existingLeave.Days);

                if (!deductSuccess)
                {
                    throw new Exception(deductMessage); // Prevents deduction if it fails
                }
            }

            // Explicitly mark the entity as modified
            _context.Entry(existingLeave).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            Console.WriteLine($"Leave status updated: {existingLeave.Approved}");
            return existingLeave;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }


    public async Task<DriverLeave> DeleteLeave(DriverLeave model)
    {
        try
        {
            _context.DriverLeaves.Remove(model);
            await _context.SaveChangesAsync();
            return model;
        }
        catch (Exception)
        {
            throw new Exception();
        }
    }
}