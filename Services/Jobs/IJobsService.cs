using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.Entities;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Models.ResponseResults.SaleOrder;
using KairosWebAPI.Models.ResponseResults.GoogleLocation;

namespace KairosWebAPI.Services.Jobs
{
    public interface IJobsService
    {
        //To Get Vehicle list Only
        Task<ServiceResponse<List<Vehicles>>> GetVehicles();
        Task<ServiceResponse<VehicleDto>> GetVehicleJourneyByDate(DateTime journeyDate, string truckNum, string truckDescription);
        Task<ServiceResponse<object>> GetVehicleSubJourneyByDate(DateTime journeyDate, string truckNum, string truckDescription);
        Task<ServiceResponse<Journey>> CreateJourney(JourneyCreatDto model);
        Task<ServiceResponse<Journey>> CreateJourneyWithSelectedOrders(JourneyCreatDto model);
        Task<ServiceResponse<JourneyDetailDto>> CreateJourneyDetail(JourneyDetailDto model);
        //To Get Vehicle List with Journey Information as well based on the Date passed
        Task<ServiceResponse<List<VehicleDto>>> GetVehicleJourneyListByDate(DateTime journeyDate);
        Task<PaginationResponse<JourneyDto>> GetList(JourneyFilterParams filter);
        Task<ServiceResponse<JourneyDto>> ChangeVehicle(JourneyDto model, string newPartNum);
        Task<ServiceResponse<string>> DeleteJourney(decimal orderNumber);
        Task<ServiceResponse<JourneyDto>> CreateJourneyFromOrderNumber(string company, decimal orderNumber);
        Task<ServiceResponse<List<VehicleDto>>> AssignJobsToTrucks(List<KFmsOrder> ordersList);
        Task<ServiceResponse<JourneyDto>> UpdateJourney(JourneyDto model);
        Task<ServiceResponse<JourneyDto>> DeleteJourney(int id);
        Task<ServiceResponse<JourneyDto>> DeleteJourneyDetail(int detailId);
        Task<ServiceResponse<JourneyDto>> UpdateJourneyTime(JourneyTimeUpdateDto model);
        Task<ServiceResponse<string>> UpdateJobStatus(int id, string status, int hoursWorked, string latitude, string longitude);
        Task<ServiceResponse<string>> UpdateSubJobStatus(int id, string status, int hoursWorked, string latitude, string longitude, int? actualHours);
        Task<ServiceResponse<object>> GetDriverCommissionDetail(string truckName, DateTime date);
        Task<ServiceResponse<string>> DownloadShipmentReport(int packNum, string fileUrl);
        Task<ServiceResponse<LocationResponse>> GetGoogleLocationDetails(string address);
        Task<ServiceResponse<dynamic>> GetJourneyDetail(long jobNumber);
        Task<ServiceResponse<dynamic>> UpdateExt(Journey journey);
        Task<ServiceResponse<List<TruckStatsDto>>> GetTruckStats(DateTime startDate, DateTime endDate);

    }
}
