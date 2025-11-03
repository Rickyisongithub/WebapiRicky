using KairosWebAPI.Models.Entities;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Services.TimeEntryService;
using Microsoft.AspNetCore.Mvc;

namespace KairosWebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TimeEntryController : Controller
{
    private readonly ITimeEntryService _timeEntryService;

    public TimeEntryController(ITimeEntryService timeEntryService)
    {
        _timeEntryService = timeEntryService;
    }

    [HttpGet("GetEmployees")]
    public async Task<IActionResult> GetEmployees([FromQuery] FilterParams filters)
    {
        return Ok(await _timeEntryService.GetEmployees(filters: filters));
    }
    
    [HttpGet("GetTimeEntriesByEmp")]
    public async Task<IActionResult> GetTimeEntriesByEmp([FromQuery] string empId, [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        return Ok(await _timeEntryService.GetTimeEntriesByEmp(empId, fromDate, toDate));
    }
    
    [HttpPost("AddTimeEntrySingle")]
    public async Task<IActionResult> AddTimeEntrySingle(TimeEntryDO timeEntryDo)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _timeEntryService.AddTimeEntrySingle(timeEntryDo));
        }

        return BadRequest("Requested data has issues");
    }
    
    [HttpPut("UpdateTimeEntrySingle")]
    public async Task<IActionResult> UpdateTimeEntrySingle(TimeEntryDO timeEntryDo)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _timeEntryService.UpdateTimeEntrySingle(timeEntryDo));
        }

        return BadRequest("Requested data has issues");
    }
    
    [HttpDelete("DeleteTimeEntrySingle")]
    public async Task<IActionResult> DeleteTimeEntrySingle(int id)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _timeEntryService.DeleteTimeEntrySingle(id));
        }

        return BadRequest("Requested data has issues");
    }
    
    [HttpPut("ApproveOrRejectEntrySingle")]
    public async Task<IActionResult> ApproveOrRejectEntrySingle(int id, bool approve, bool reject)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _timeEntryService.ApproveOrRejectEntrySingle(id, approve, reject));
        }

        return BadRequest("Requested data has issues");
    }
}