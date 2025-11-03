using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.FilterDtos;

namespace KairosWebAPI.Services.Vendor
{
    public interface IVendorService
    {
        Task<Dictionary<string, string>> GetVendorsList(FilterParams filters);
        Task<ServiceResponse<string>> UpdateVendor(UpdateVendorDto model, string company, int vendorNum);
    }
}
