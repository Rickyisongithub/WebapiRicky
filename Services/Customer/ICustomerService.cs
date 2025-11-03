using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Models.ResponseResults;

namespace KairosWebAPI.Services.Customer
{
    public interface ICustomerService
    {
        Task<ServiceResponse<KListResponse<CustomerDto>>> GetCustomerList(FilterParams filters);
    }
}
