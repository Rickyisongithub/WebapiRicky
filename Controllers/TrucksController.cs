using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Services.Trucks;
using Microsoft.AspNetCore.Mvc;

namespace KairosWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  //  [ServiceFilter(typeof(KAuthorizationFilter))]
    public class TrucksController : ControllerBase
    {
        private readonly ITrucksService _trucksService;

        public TrucksController(ITrucksService trucksService)
        {
            this._trucksService = trucksService;
        }
        [HttpGet("GetTrucks")]
        public async Task<IActionResult> GetTrucksList([FromQuery] FilterParams filters)
        {
            return Ok(await _trucksService.GetTrucksList(filters));
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetTrucksDbList([FromQuery] FilterParams filters)
        {
            return Ok(await _trucksService.GetDbTrucks(filters));
        }
        [HttpPut("update-location")]
        public async Task<IActionResult> UpdateTruckLocation([FromBody] UpdateTruckLocationDto model)
        {
            return Ok(await _trucksService.UpdateTruckLocation(model.PartNum!,model.Location!));
        }
        [HttpGet("detail/{partNum}")]
        public async Task<IActionResult> GetTruckDetails(string partNum)
        {
            return Ok(await _trucksService.GetTruckDetails(partNum));
        }


    }
}