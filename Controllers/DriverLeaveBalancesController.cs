using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.Entities;
using KairosWebAPI.Services.DriverLeaveBalanceService;
using Microsoft.AspNetCore.Mvc;

namespace KairosWebAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DriverLeaveBalancesController : ControllerBase
    {
        private readonly IDriverLeaveBalanceService _balanceService;

        public DriverLeaveBalancesController(IDriverLeaveBalanceService balanceService)
        {
            _balanceService = balanceService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllBalances()
        {
            var balances = await _balanceService.GetAllBalances();
            return Ok(balances);
        }

        [HttpGet("GetBalance")]
        public async Task<IActionResult> GetBalance(string driverName, string leaveType)
        {
            var balance = await _balanceService.GetBalanceByDriverAndType(driverName, leaveType);
            if (balance == null) return NotFound("Balance record not found");
            return Ok(balance);
        }

        [HttpPost("AddOrUpdateBalance")]
        public async Task<IActionResult> AddOrUpdateBalance(DriverLeaveBalance balance)
        {
            var updatedBalance = await _balanceService.AddOrUpdateBalance(balance);
            return Ok(updatedBalance);
        }

        [HttpPatch("DeductLeave")]
        public async Task<IActionResult> DeductLeave(string driverName, string leaveType)
        {
            var success = await _balanceService.DeductLeave(driverName, leaveType);
            if (!success) return BadRequest("No leave balance available");
            return Ok("Leave deducted successfully");
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestLeave([FromBody] LeaveRequestDto request)
        {
            var (success, message) = await _balanceService.RequestLeave(request.DriverName, request.LeaveType, request.DaysRequested);
            if (success)
            {
                return Ok(new { message });
            }
            return BadRequest(new { message });
        }

        [HttpPost("get")]
        public async Task<IActionResult> GetLeaveBalances([FromBody] LeaveBalanceRequestDto request)
        {
            var result = await _balanceService.GetLeaveBalances(request.From, request.To, request.Trucks);
            return Ok(result);
        }
    }
}