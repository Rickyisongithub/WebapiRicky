using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.Entities;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Models.ResponseResults.SaleOrder;
using KairosWebAPI.Services;
using KairosWebAPI.Services.Jobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace KairosWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {

        private readonly IJobsService _jobsService;
        private readonly IHubContext<JourneyHub> _hubContext;

        public JobsController(IJobsService jobsService, IHubContext<JourneyHub> hubContext)
        {
            _jobsService = jobsService;
            _hubContext = hubContext;
        }

        [HttpGet("GetVehicles")]
        public async Task<IActionResult> GetVehicles()
        {
            return Ok(await _jobsService.GetVehicles());
        }

        [HttpGet("GetVehicleJourneyByDate")]
        public async Task<IActionResult> GetVehicles(DateTime date, string truckNum, string truckDescription)
        {
            return Ok(await _jobsService.GetVehicleJourneyByDate(date, truckNum, truckDescription));
        }
        [HttpGet("GetVehicleSubJourneyByDate")]
        public async Task<IActionResult> GetSubVehicles(DateTime date, string truckNum, string truckDescription)
        {
            return Ok(await _jobsService.GetVehicleSubJourneyByDate(date, truckNum, truckDescription));
        }

        [HttpGet("GetVehiclesList/{journeyDate}")]
        public async Task<IActionResult> GetCustomerList(DateTime journeyDate)
        {
            return Ok(await _jobsService.GetVehicleJourneyListByDate(journeyDate));
        }

        [HttpPost("CreateJourney")]
        public async Task<IActionResult> CreateJourney(JourneyCreatDto model)
        {
            var result = await _jobsService.CreateJourney(model);

            if (result is not { Success: true, Data: not null }) return BadRequest(result);
            var responseObj = new
            {
                JourneyId = result.Data.Id,
                result.Data.CustNum,
                result.Data.CustomerId,
                result.Data.CustomerName,
            };
            await _hubContext.Clients.All.SendAsync("JourneyMethodReceiveOne","JourneyCreated", responseObj);
            return Ok(result);
        }
        
        [HttpPost("CreateJourneyWithSelectedOrders")]
        public async Task<IActionResult> CreateJourneyWithSelectedOrders(JourneyCreatDto model)
        {
            var result = await _jobsService.CreateJourneyWithSelectedOrders(model);

            if (result is not { Success: true, Data: not null }) return BadRequest(result);
            var responseObj = new
            {
                JourneyId = result.Data.Id,
                result.Data.CustNum,
                result.Data.CustomerId,
                result.Data.CustomerName,
            };
            await _hubContext.Clients.All.SendAsync("JourneyMethodReceiveOne","JourneyCreated", responseObj);
            return Ok(result);
        }

        [HttpPost("CreateJourneyDetail")]
        public async Task<IActionResult> CreateJourneyDetail(JourneyDetailDto model)
        {
            var result = await _jobsService.CreateJourneyDetail(model);
            if (result is not { Success: true, Data: not null }) return BadRequest(result);
            var responseObj = new
            {
                JourneyDetailId = result.Data.Id,
                result.Data.Location,
                result.Data.StartDate,
                result.Data.EndDate,
            };
            await _hubContext.Clients.All.SendAsync("JourneyMethodReceiveOne","JourneyDetailCreated",responseObj);
            return Ok(result);
        }

        [HttpPost("Change-Truck/{newPartNum}")]
        public async Task<IActionResult> ChangeVehicle(JourneyDto model,string newPartNum)
        {
            return Ok(await _jobsService.ChangeVehicle(model,newPartNum));
        }

        [HttpGet("Get-list")]
        public async Task<IActionResult> GetJourneyList([FromQuery] JourneyFilterParams filters)
        {
            return Ok(await _jobsService.GetList(filters));
        }

        [HttpPut("update-journey-time")]
        public async Task<IActionResult> UpdateJourneyTime(JourneyTimeUpdateDto journeyTime)
        {
            var result = await _jobsService.UpdateJourneyTime(journeyTime);
            if (result is not { Success: true, Data: not null }) return BadRequest(result);
            var responseObj = new
            {
                JourneyId = result.Data.Id,
                result.Data.TravelStartTime,
                result.Data.TravelEndTime,
            };
            await _hubContext.Clients.All.SendAsync("JourneyMethodReceiveOne","JourneyTimeUpdated", responseObj);
            return Ok(result);
        }

        [HttpDelete("delete-journey/{id}")]
        public async Task<IActionResult> DeleteJourney(int id)
        {
            var result = await _jobsService.DeleteJourney(id);
            if (result is not { Success: true, Data: not null }) return BadRequest(result);
            var responseObj = new
            {
                JourneyId = result.Data.Id,
                result.Data.CustNum,
                result.Data.CustomerId,
                result.Data.CustomerName,
            };
            await _hubContext.Clients.All.SendAsync("JourneyMethodReceiveOne","JourneyDeleted", responseObj);
            return Ok(result);
        }

        [HttpDelete("delete-journey-detail/{detailId}")]
        public async Task<IActionResult> DeleteJourneyDetail(int detailId)
        {
            var result = await _jobsService.DeleteJourneyDetail(detailId);
            if (!result.Success || result.Data == null) return BadRequest(result);
            var responseObj = new
            {
                JourneyDetailId = result.Data.Id,
                result.Data.StartDate,
                result.Data.EndDate,
            };
            await _hubContext.Clients.All.SendAsync("JourneyMethodReceiveOne","JourneyDetailDeleted", responseObj);
            return Ok(result);
        }

        [HttpPost("auto-assign-jobs")]
        public async Task<IActionResult> AutoAssignJobs(List<KFmsOrder> orders)
        {
            return Ok(await _jobsService.AssignJobsToTrucks(orders));
        }

        [HttpPut("UpdateJobStatus")]
        public async Task<IActionResult> UpdateJobStatus(int id, string status, int hoursWorked, string latitude, string longitude)
        {
            var result = await _jobsService.UpdateJobStatus(id, status, hoursWorked, latitude, longitude);
            if (result is not { Success: true, Data: not null }) return BadRequest(result);
            await _hubContext.Clients.All.SendAsync("JourneyMethodReceiveOne","StatusUpdated",result);
            return Ok(result);
        }
        
        [HttpPut("UpdateSubJobStatus")]
        public async Task<IActionResult> UpdateSubJobStatus(int id, string status, int hoursWorked, string latitude, string longitude, int? actualHours)
        {
            var result = await _jobsService.UpdateSubJobStatus(id, status, hoursWorked, latitude, longitude, actualHours);
            if (result is not { Success: true, Data: not null }) return BadRequest(result);
            await _hubContext.Clients.All.SendAsync("JourneyMethodReceiveOne","StatusUpdated",result);
            return Ok(result);
        }
        
        [HttpGet("GetDriverCommissionDetail")]
        public async Task<IActionResult> GetDriverCommissionDetail(string truckName, DateTime date)
        {
            var result = await _jobsService.GetDriverCommissionDetail(truckName, date);
            if (result is not { Success: true, Data: not null }) return BadRequest(result);
            return Ok(result);
        }
        
        [HttpGet("download-shipment-report/{packNum}")]
        public async Task<IActionResult> DownloadShipmentReport(int packNum)
        {
            var fileUrl = Request.Scheme + "://" + Request.Host + "/kairoswebapi/Files/";
            return Ok(await _jobsService.DownloadShipmentReport(packNum,fileUrl));
        }
        
        [HttpGet("get-location/{address}")]
        public async Task<IActionResult> GetGoogleLocationDetails(string address)
        {
            return Ok(await _jobsService.GetGoogleLocationDetails(address));
        }
        
        [HttpGet("get-job-detail/{id}")]
        public async Task<IActionResult> GetJobDetail(long id)
        {
            return Ok(await _jobsService.GetJourneyDetail(id));
        }
        
        [HttpPost("UpdateJobExt")]
        public async Task<IActionResult> UpdateJobExt(Journey journey)
        {
            var result = await _jobsService.UpdateExt(journey);
            if (!result.Success || result.Data == null) return BadRequest(result);
            await _hubContext.Clients.All.SendAsync("JourneyMethodReceiveOne");
            return Ok(result);
        }

        [HttpGet("truck-stats")]
        public async Task<IActionResult> GetTruckStats([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return Ok(await _jobsService.GetTruckStats(startDate, endDate));
        }
    }
}

