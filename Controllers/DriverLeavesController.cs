using KairosWebAPI.Models.Entities;
using KairosWebAPI.Services.DriverLeaveBalanceService;
using KairosWebAPI.Services.DriverLeaveService;
using KairosWebAPI.Services.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace KairosWebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DriverLeavesController : Controller
{
    // GET
    private readonly IJobsService _jobsService;
    private readonly IDriverLeaveService _driverLeaveService;
    private readonly IDriverLeaveBalanceService _balanceService;

    public DriverLeavesController(IJobsService jobsService, IDriverLeaveService driverLeaveService, IDriverLeaveBalanceService balanceService)
    {
        _jobsService = jobsService;
        _driverLeaveService = driverLeaveService;
        _balanceService = balanceService;
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(DateTime? from, DateTime? to, string? driverName, string? leaveType)
    {

        var fromDate = from ?? new DateTime(DateTime.Now.Year, 1, 1);  // Start of current year
        var toDate = to ?? new DateTime(DateTime.Now.Year, 12, 31);    // End of current year

        var vehicles = await _jobsService.GetVehicles();
        var driverLeaves = await _driverLeaveService.GetAllDriverLeavesWithFilters(fromDate, toDate, driverName, leaveType);

        if (vehicles.Data == null)
            return Ok(await _jobsService.GetVehicles());

        var result = new List<object>();

        foreach (var cVehicle in vehicles.Data)
        {

            if (!string.IsNullOrEmpty(driverName) && !cVehicle.Description.Equals(driverName, StringComparison.OrdinalIgnoreCase))
                continue;

            // Fetch all leave balances for each driver
            var leaveBalances = await _balanceService.GetBalanceByDriver(cVehicle.Description); // Adjust the leaveType as needed

            // Create the result object
            var vehicleResult = new
            {
                cVehicle.PartNum,
                cVehicle.Description,
                Leaves = driverLeaves.Where(c => c.DriverName == cVehicle.Description).ToList(),
                LeaveBalances = leaveBalances // Embedding all leave balances in the response
            };

            result.Add(vehicleResult);
        }

        return Ok(result);
    }


    [HttpPost("AddLeave")]
    public async Task<IActionResult> AddLeave(DriverLeave driverLeave)
    {
        try
        {
            var result = await _driverLeaveService.AddLeave(driverLeave);
            return Ok(new { success = true, message = "Leave request processed successfully.", data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", details = ex.Message });
        }
    }
    
    [HttpPatch("UpdateLeave")]
    public async Task<IActionResult> UpdateLeave(DriverLeave driverLeave)
    {
        var result = await _driverLeaveService.UpdateLeave(driverLeave);
        return Ok(result);
    }
    
    [HttpDelete("DeleteLeave")]
    public async Task<IActionResult> DeleteLeave(DriverLeave driverLeave)
    {
        var result = await _driverLeaveService.DeleteLeave(driverLeave);
        return Ok(result);
    }
}