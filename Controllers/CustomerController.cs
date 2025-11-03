using KairosWebAPI.AuthorizationFilter;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Services.Customer;
using Microsoft.AspNetCore.Mvc;

namespace KairosWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(KAuthorizationFilter))]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            this._customerService = customerService;
        }
        [HttpGet("GetCustomers")]
        public async Task<IActionResult> GetCustomerList([FromQuery] FilterParams filters)
        {
            return Ok(await _customerService.GetCustomerList(filters));
        }

      
    }
}

