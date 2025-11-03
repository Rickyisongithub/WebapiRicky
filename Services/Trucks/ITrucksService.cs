using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.Entities;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Models.ResponseResults;

namespace KairosWebAPI.Services.Trucks
{
    public interface ITrucksService
    {
        Task<ServiceResponse<KListResponse<TrucksDto>>> GetTrucksList(FilterParams filters);
        Task<ServiceResponse<List<Truck>>> GetDbTrucks(FilterParams filters);
        Task<ServiceResponse<Truck>> UpdateTruckLocation(string partNum, string location);
        Task<ServiceResponse<TruckDetailDto>> GetTruckDetails(string partNum);

    }
}
