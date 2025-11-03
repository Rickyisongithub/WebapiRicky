using KairosWebAPI.AuthorizationFilter;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.FilterDtos;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using KairosWebAPI.Services.Vendor;

namespace KairosWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    [ServiceFilter(typeof(KAuthorizationFilter))]
    public class VendorController : ControllerBase
    {
        private readonly IVendorService _vendorService;

        public VendorController(IVendorService vendorService)
        {
            this._vendorService = vendorService;
        }
                
        [HttpGet]
        public async Task<IActionResult> GetVendorsList([FromQuery] FilterParams filters)
        {
            Dictionary<string, string> response = await _vendorService.GetVendorsList(filters);
            var doc = JsonDocument.Parse(response["Body"]);
            return Ok(doc);
        }

        [HttpPatch("Update/{company}/{vendorNum}")]
        public async Task<IActionResult> GetVendorsList(string company,int vendorNum,UpdateVendorDto vendor)
        {
            vendor.Company = company;
            vendor.VendorNum = vendorNum;
            return Ok(await _vendorService.UpdateVendor(vendor, company, vendorNum));
        }
    }
}
