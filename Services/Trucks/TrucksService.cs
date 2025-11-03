using KairosWebAPI.DatabaseContext;
using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.Entities;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Models.ResponseResults;
using Microsoft.EntityFrameworkCore;
using RestSharp;

namespace KairosWebAPI.Services.Trucks
{
    public class TrucksService:ITrucksService
    {
        private readonly RestClient _restClient;
        private readonly AppDbContext _context;
        private readonly string _apiKey;
        private readonly string _authorizationKey;
        public TrucksService(IConfiguration config,AppDbContext context)
        {
            var configuration = config;
            this._context = context;
            _restClient = new RestClient(configuration["kairos:API_Url"] ?? string.Empty);
            _apiKey = configuration["kairos:API_KEY"] ?? string.Empty;
            _authorizationKey = configuration["kairos:AUTHORIZATION_KEY"] ?? string.Empty;
        }

        public async Task<ServiceResponse<KListResponse<TrucksDto>>> GetTrucksList(FilterParams filters)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
                return ServiceResponse<KListResponse<TrucksDto>>.Return409("{\"message\":\"Missing API Keys in App Settings\"}");


            var request = new RestRequest("BaqSvc/K_DEM_MOVERS/Data");
            request.Timeout = -1;
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");

            if (!string.IsNullOrWhiteSpace(filters.Select)) request.AddParameter("$select", filters.Select);
            if (!string.IsNullOrWhiteSpace(filters.Expand)) request.AddQueryParameter("$expand", filters.Expand);
            if (!string.IsNullOrWhiteSpace(filters.Filter)) request.AddQueryParameter("$filter", filters.Filter);
            if (!string.IsNullOrWhiteSpace(filters.OrderBy)) request.AddQueryParameter("$orderBy", filters.OrderBy);
            if (filters.Top.GetValueOrDefault() != default) request.AddQueryParameter("$top", filters.Top.ToString());
            if (filters.Skip.GetValueOrDefault() != default) request.AddQueryParameter("$skip", filters.Skip.ToString());
            if (filters.Count.GetValueOrDefault() != default) request.AddQueryParameter("$count", filters.Count.ToString());

            var response = await _restClient.ExecuteGetAsync<KListResponse<TrucksDto>>(request);
            if (response.IsSuccessful && response.Data != null)
            {
                if (response.Data.value?.Count > 0)
                {
                    var dbTrucks = await _context.Trucks.AsNoTracking().Select(x => x.Part_PartNum).ToListAsync();
                    var trucksToAdd = response.Data.value.Where(x => !dbTrucks.Contains(x.Part_PartNum)).Select(x => new Truck()
                    {
                        Part_PartNum = x.Part_PartNum,
                        Part_PartDescription = x.Part_PartDescription,
                        Commission = 0,
                        Part_SalesUM = x.Part_SalesUM,
                        Part_UnitPrice = x.Part_UnitPrice,
                        Part_Company = x.Part_Company,
                        Status = "active",
                        CurrentLocation = null
                    }).ToList();
                    await _context.Trucks.AddRangeAsync(trucksToAdd);
                    await _context.SaveChangesAsync();
                }
                return ServiceResponse<KListResponse<TrucksDto>>.ReturnResultWith200(response.Data);

            }

            return ServiceResponse<KListResponse<TrucksDto>>.ReturnFailed((int)response.StatusCode, response.ErrorMessage ?? "Failed");
        }
   
        public async Task<ServiceResponse<List<Truck>>> GetDbTrucks(FilterParams filters)
        {
            var result  = await _context.Trucks.Skip(filters.Skip.GetValueOrDefault()).Take(filters.Top?? 50).ToListAsync();
            return ServiceResponse<List<Truck>>.ReturnResultWith200(result);
        }
        public async Task<ServiceResponse<Truck>> UpdateTruckLocation(string partNum,string location)
        {
            var truck =  await _context.Trucks.FirstOrDefaultAsync(x => x.Part_PartNum== partNum);
            if (truck == null) return ServiceResponse<Truck>.Return422("Truck not Found");
            
            truck.CurrentLocation= location;
            _context.Trucks.Update(truck);
            await _context.SaveChangesAsync();
            return ServiceResponse<Truck>.ReturnResultWith200(truck);

        }

        public async Task<ServiceResponse<TruckDetailDto>> GetTruckDetails(string partNum)
        {
            var truck = await _context.Trucks.FirstOrDefaultAsync(x => x.Part_PartNum == partNum);
            if (truck == null) return ServiceResponse<TruckDetailDto>.Return422("Truck not Found");

            var allOrders = await _context.Journeys.Where(x => x.VehicleNum == partNum).AsNoTracking().OrderBy(x => x.NeedByDate).ToListAsync();
           
            var ordersHistory = allOrders.Where(x => x.Status == "COMPLETED").ToList();
            
            var assignedOrder = allOrders.FirstOrDefault(x => x.Status != "COMPLETED");
            
            JourneyInformationDto journeyDetail =  new JourneyInformationDto();
            if(assignedOrder != null)
            {
                var job = await _context.Journeys.Where(x => x.Id == assignedOrder.Id)
                        .Include(x => x.JourneyDetails)
                        .FirstOrDefaultAsync();
                var truckDetails = await _context.JourneyTrucks.Where(x => x.JourneyId == assignedOrder.Id)
                        .Include(x => x.TruckLocations)
                        .ToListAsync();


                journeyDetail.Job= job;
                journeyDetail.TruckDetails= truckDetails;
            }
        
            return ServiceResponse<TruckDetailDto>.ReturnResultWith200(new TruckDetailDto()
            {
                Truck = truck,
                JobHistory = ordersHistory,
                Job = journeyDetail
            });
        }

        
    }

}

