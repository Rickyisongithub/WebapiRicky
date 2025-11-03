using AutoMapper;
using KairosWebAPI.DatabaseContext;
using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.Entities;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Models.ResponseResults;
using KairosWebAPI.Models.ResponseResults.GoogleLocation;
using KairosWebAPI.Models.ResponseResults.SaleOrder;
using KairosWebAPI.Models.ResponseResults.Shipment;
using KairosWebAPI.Services.Logs;
using KairosWebAPI.Services.Order;
using KairosWebAPI.Services.Trucks;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Text.Json;
using KairosWebAPI.Services.FileService;

namespace KairosWebAPI.Services.Jobs
{
    public class JobsService : IJobsService
    {
        private readonly RestClient _restClient;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;
        private readonly ITrucksService _trucksService;
        private readonly IFileService _fileService;
        private readonly ILogService _logger;
        private readonly string _apiKey;
        private readonly string _authorizationKey;
        private readonly int _vehicleHours;
        private readonly string _startLocation;


        public JobsService(IConfiguration config, AppDbContext context, IMapper mapper, IOrderService orderService,
            ITrucksService trucksService
            ,IFileService fileService,
            ILogService logger)
        {
            _context = context;
            _mapper = mapper;
            _orderService = orderService;
            _trucksService = trucksService;
            _fileService = fileService;
            _logger = logger;
            _restClient = new RestClient(config["kairos:API_Url"] ?? string.Empty);
            _apiKey = config["kairos:API_KEY"] ?? string.Empty;
            _authorizationKey = config["kairos:AUTHORIZATION_KEY"] ?? string.Empty;
            _vehicleHours = Convert.ToInt32(config["kairos:VehicleHours"] ?? "0");
            _startLocation = config["kairos:StartLocation"] ?? "no-location";

        }

        public async Task<ServiceResponse<List<Vehicles>>> GetVehicles()
        {
            var vehiclesResponse = await GetTrucksList(new FilterParams());
            if (vehiclesResponse is not { Success: true, Data: not null })
                return ServiceResponse<List<Vehicles>>.Return409("No Vehicles Found");
            var trucks = vehiclesResponse.Data.value;
            var vehicles = trucks!.Select(truck => new Vehicles()
                { PartNum = truck.Part_PartNum, Description = truck.Part_PartDescription }).ToList();
            return ServiceResponse<List<Vehicles>>.ReturnResultWith200(vehicles);
        }

        public async Task<ServiceResponse<VehicleDto>> GetVehicleJourneyByDate(DateTime journeyDate, string truckNum,
            string truckDescription)
        {
            var result = await _context.Journeys
                .Include(x => x.JourneyDetails).Include(x => x.JourneyTrucks)
                .ThenInclude(x => x.TruckLocations)
                .Where(x => x.StartDate!.Value.Date == journeyDate.Date && x.VehicleNum == truckNum).AsNoTracking()
                .ToListAsync();
            // var journeyTrucks = await _context.Journeys.Include(x => x.JourneyTrucks)
            //     .ThenInclude(x => x.TruckLocations)
            //     .Where(x => x.JourneyTrucks != null && x.StartDate!.Value.Date == journeyDate.Date &&
            //                 x.JourneyTrucks.FirstOrDefault(c => c.TruckId == truckNum) != null).AsNoTracking()
            //     .ToListAsync();
            // foreach (var journeyTruck in journeyTrucks)
            // {
            //     if (journeyTruck.JourneyTrucks == null) continue;
            //     foreach (var journeyTruckJourneyTruck in journeyTruck.JourneyTrucks)
            //     {
            //         if (journeyTruckJourneyTruck.EndLocation == null) continue;
            //         var formattedAddress = await HelperMethods.GetFormattedAddress(
            //             journeyTruckJourneyTruck.EndLocation.Split("~")[0],
            //             journeyTruckJourneyTruck.EndLocation.Split("~")[1]);
            //         journeyTruck.CustomerAddress = formattedAddress;
            //     }
            // }
            // result.AddRange(journeyTrucks);
            // var journeyList = new List<Journey>();
            var vehicleDto = new VehicleDto
            {
                PartNum = truckNum,
                Description = truckDescription,
                Journeys = result,
            };
            return ServiceResponse<VehicleDto>.ReturnResultWith200(vehicleDto);
        }
        
        public async Task<ServiceResponse<object>> GetVehicleSubJourneyByDate(DateTime journeyDate, string truckNum,
            string truckDescription)
        {
            var result = await _context.JourneyTrucks.Include(c => c.Journey)
                .ThenInclude(c => c!.JourneyDetails).Include(c => c.TruckLocations)
                .Where(c => c.Journey != null && c.Journey.StartDate != null && c.TruckId == truckNum &&
                            c.Journey.StartDate.Value.Date == journeyDate.Date)
                .AsNoTracking().ToListAsync();
            var vehicleDto = new
            {
                PartNum = truckNum,
                Description = truckDescription,
                Journeys = result,
            };
            return ServiceResponse<object>.ReturnResultWith200(vehicleDto);
        }

        public async Task<ServiceResponse<Journey>> CreateJourney(JourneyCreatDto model)
        {
            var salesOrder = new SalesOrder()
            {
                Company = model.Company,
                CustomerCustID = model.CustomerId,
                RequestDate = model.JourneyDate,
                OrderDate = model.JourneyDate,
                NeedByDate = model.JourneyDate,
                TermsCode = "30EM",
                UseOTS = model.UseOTS,
                CustNum = model.CustomerNum,
                OTSName = "OTS"+DateTime.UtcNow.Date.ToString("yyyyMMdd"),
                OTSAddress1 = model.OTSAddress1,
                OTSAddress2 = model.OTSAddress2,
                OTSCity= model.OTSCity,
                OTSZIP =  model.OTSZip,
                PONum = model.PONum,
                EntryPerson = model.EntryPerson,
                OrderDtls = new List<OrderDtl>()
                {
                    new()
                    {
                        PartNum = "Type_1",
                        OrderQty = model.Hours,
                        Company = model.Company,
                        LineType = "PART",
                        LineDesc = "Type_1",
                    }
                }
            };

            var result = await CreateSalesOrder(salesOrder);
            if (result is not { Success: true, Data: not null })
                return ServiceResponse<Journey>.ReturnFailed(result.StatusCode, result.Errors);
            var dt = result.Data;
            var details = _mapper.Map<List<JourneyDetail>>(model.JourneyDetails);
            //var location = await HelperMethods.GetLongLatInStringByAddress(model.CustomerAddress!); 
            var job = new Journey()
            {
                OrderNum = dt.OrderNum,
                Company = dt.Company,
                CustNum = dt.CustNum,
                CustomerId = dt.CustomerCustID,
                CustomerName = dt.CustomerName,
                CustomerAddress = model.CustomerAddress,
                OrderDate = dt.OrderDate,
                PartNum = "Type_1",
                VehicleNum = model.VehicleNum, 
                StartDate = model.JourneyDate,
                TravelStartTime = model.StartTime,
                TravelEndTime = model.StartTime.GetValueOrDefault().AddHours(model.Hours),
                EndDate = model.JourneyDate.GetValueOrDefault().AddHours(model.Hours),
                Hours = model.Hours,    
                StartLocation = model.StartLocation,
                EndLocation= model.EndLocation,
                NeedByDate = dt.NeedByDate,
                RequestDate = dt.RequestDate,
                JourneyDetails = details, //intermediate_locations
                Status = "ASSIGNED",
                PONum = dt.PONum,
                EntryPerson = dt.EntryPerson,
                OrderHed_SysRowId = dt.SysRowID,
            };
            await _context.Journeys.AddAsync(job);
            var truck = await _context.Trucks.FirstOrDefaultAsync(x => x.Part_PartNum == model.VehicleNum);
            if(truck != null)
            {
                truck.CurrentLocation = model.StartLocation;
                _context.Trucks.Update(truck);
            }

            await _context.SaveChangesAsync();
            if (model.JourneyTrucks is { Count: > 0 })
            {
                var journeyTrucks= MapJourneyTrucks(job.Id,model.JourneyTrucks);
                await _context.JourneyTrucks.AddRangeAsync(journeyTrucks);
                await _context.SaveChangesAsync();
            }

            await _logger.LogInformation(new FMSLogs()
            {
                Description = $"Journey Created With Job Id {job.Id}, and Job Num {job.OrderNum}",
                JobNum = job.Id,
                OrderNum = (long)job.OrderNum!,
                Location = job.EndLocation,
                LogType = LogTypeEnum.Create.ToString(),
                PackNum = job.PackNum ?? 0 ,
                Status = job.Status
            });

            return ServiceResponse<Journey>.ReturnResultWith201(job);

        }
        
        public async Task<ServiceResponse<Journey>> CreateJourneyWithSelectedOrders(JourneyCreatDto model)
        {
            var result = await _orderService.GetOrderByNumber(model.Company ?? "", model.OrderNum ?? 0);
            if (result is not { Success: true, Data: not null })
                return ServiceResponse<Journey>.ReturnFailed(result.StatusCode, result.Errors);
            var dt = result.Data;
            var details = _mapper.Map<List<JourneyDetail>>(model.JourneyDetails);
            //var location = await HelperMethods.GetLongLatInStringByAddress(model.CustomerAddress!); 
            var job = new Journey()
            {
                OrderNum = dt.OrderNum,
                Company = dt.Company,
                CustNum = dt.CustNum,
                CustomerId = dt.CustomerCustID,
                CustomerName = dt.CustomerName,
                CustomerAddress = model.CustomerAddress,
                OrderDate = dt.OrderDate,
                PartNum = "Type_1",
                VehicleNum = model.VehicleNum, 
                StartDate = model.JourneyDate,
                TravelStartTime = model.StartTime,
                TravelEndTime = model.StartTime.GetValueOrDefault().AddHours(model.Hours),
                EndDate = model.JourneyDate.GetValueOrDefault().AddHours(model.Hours),
                Hours = model.Hours,    
                StartLocation = model.StartLocation,
                EndLocation= model.EndLocation,
                NeedByDate = dt.NeedByDate,
                RequestDate = dt.RequestDate,
                JourneyDetails = details, //intermediate_locations
                Status = "ASSIGNED",
                PONum = dt.PONum,
                EntryPerson = dt.EntryPerson,
                OrderHed_SysRowId = dt.SysRowID,
            };
            await _context.Journeys.AddAsync(job);
            var truck = await _context.Trucks.FirstOrDefaultAsync(x => x.Part_PartNum == model.VehicleNum);
            if(truck != null)
            {
                truck.CurrentLocation = model.StartLocation;
                _context.Trucks.Update(truck);
            }

            await _context.SaveChangesAsync();
            if (model.JourneyTrucks is { Count: > 0 })
            {
                var journeyTrucks= MapJourneyTrucks(job.Id,model.JourneyTrucks);
                await _context.JourneyTrucks.AddRangeAsync(journeyTrucks);
                await _context.SaveChangesAsync();
            }

            await _logger.LogInformation(new FMSLogs()
            {
                Description = $"Journey Created With Job Id {job.Id}, and Job Num {job.OrderNum}",
                JobNum = job.Id,
                OrderNum = (long)job.OrderNum!,
                Location = job.EndLocation,
                LogType = LogTypeEnum.Create.ToString(),
                PackNum = job.PackNum ?? 0 ,
                Status = job.Status
            });

            return ServiceResponse<Journey>.ReturnResultWith201(job);

        }
        public async Task<ServiceResponse<dynamic>> GetJourneyDetail(long jobNumber) 
        {
            var job = await _context.Journeys.Where(x => x.Id == jobNumber)
                .Include(x => x.JourneyDetails)
                .FirstOrDefaultAsync();
            var truckDetails = await _context.JourneyTrucks.Where(x => x.JourneyId == jobNumber)
                .Include(x => x.TruckLocations)
                .ToListAsync();
            var result = new
            {
                Job = job,
                TruckDetails = truckDetails,
            };
            return ServiceResponse<dynamic>.ReturnResultWith200(result);

        }

        public async Task<ServiceResponse<JourneyDetailDto>> CreateJourneyDetail(JourneyDetailDto model)
        {
            var data = _mapper.Map<JourneyDetail>(model);
            await _context.JourneyDetails.AddAsync(data);
            await _context.SaveChangesAsync();
            model.Id = data.Id;
            await _logger.LogInformation(new FMSLogs()
            {
                Description = $"Journey Detail Create with Id: {data.Id}, and Hours {data.Hours}",
                JobNum = data.JourneyId,
                Location = data.Location,
                LogType = LogTypeEnum.Create.ToString(),
            });
            return ServiceResponse<JourneyDetailDto>.ReturnResultWith201(model);
        }

        private static IEnumerable<JourneyTruck> MapJourneyTrucks(long jobId, List<JourneyTruckDto> journeyTrucks)
        {
            var result = new List<JourneyTruck>();
            foreach (var item in journeyTrucks)
            {
                var journeyTruck = new JourneyTruck()
                {
                    JourneyId = jobId,
                    Hours = item.Hours,
                    StartLocation = item.StartLocation,
                    EndLocation = item.EndLocation,
                    TruckId = item.TruckId,
                    Status = "ASSIGNED",
                };
                if (item.TruckLocations != null && item.TruckLocations.Any())
                {
                    journeyTruck.TruckLocations = item.TruckLocations.Select(x => new JourneyTruckLocation()
                    {
                        Hours = x.Hours,
                        Location = x.Location,
                        Reason = x.Reason,
                    }).ToList();
                }
                result.Add(journeyTruck);
            }
            return result;
        }

        public async Task<ServiceResponse<List<VehicleDto>>> GetVehicleJourneyListByDate(DateTime journeyDate)
        {
            var vehiclesResponse = await GetTrucksList(new FilterParams());
            List<VehicleDto> responseResultList = new();
            if (vehiclesResponse is not { Success: true, Data: not null })
                return ServiceResponse<List<VehicleDto>>.ReturnResultWith200(responseResultList);
            var trucks = vehiclesResponse.Data.value;
            foreach(var truck in trucks!)
            {
                var result = await _context.Journeys
                    .Where(x => x.StartDate!.Value.Date == journeyDate.Date && x.VehicleNum == truck.Part_PartNum)
                    .Include(x => x.JourneyDetails).Include(navigationPropertyPath: x => x.JourneyTrucks)
                    .ThenInclude(d => d.TruckLocations).AsNoTracking().ToListAsync();
                responseResultList.Add(new VehicleDto()
                {
                    PartNum = truck.Part_PartNum,
                    Description = truck.Part_PartDescription,
                    Journeys = result
                });
            }
            return ServiceResponse<List<VehicleDto>>.ReturnResultWith200(responseResultList);
        }

        public async Task<ServiceResponse<JourneyDto>> ChangeVehicle(JourneyDto model,string newPartNum)
        {
            var journey = await _context.Journeys.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (journey == null) return ServiceResponse<JourneyDto>.Return409("Invalid Journey Id");
            journey.TravelStartTime = model.TravelStartTime;
            journey.TravelEndTime = model.TravelEndTime;
            journey.Hours = (int) model.TravelEndTime!.Value.Subtract(model.TravelStartTime!.Value).TotalHours;
            journey.VehicleNum = newPartNum;
            _context.Entry(journey).State= EntityState.Modified;

            await _context.SaveChangesAsync();
            try
            {
                var description = $"Journey Vehicle Changed from: {model.VehicleNum} to {newPartNum}";
                await _logger.LogInformation(new FMSLogs()
                {
                    Description = description,
                    JobNum = model.Id,
                    OrderNum = (int)model.OrderNum!,
                    Location = model.StartLocation,
                    Status = journey.Status,
                    LogType = LogTypeEnum.Update.ToString(),
                });

                await _logger.LogUd105(journey.Company!, journey.OrderNum.GetValueOrDefault(), journey.VehicleNum!,
                    description, journey.StartLocation!, journey.EndLocation ?? "", journey.Status!, journey.Id,
                    journey.CurrentLocation ?? "");

            }
            catch (Exception ex)
            {
                await _logger.LogExceptionAsync(ex);
            }
                

            var response = _mapper.Map<JourneyDto>(journey);
            return ServiceResponse<JourneyDto>.ReturnResultWith201(response);
        }

        public async Task<ServiceResponse<JourneyDto>> UpdateJourneyHours(int hours, decimal orderNum)
        {
            var journey = await _context.Journeys.FirstOrDefaultAsync(x => x.OrderNum == orderNum);
            if (journey == null) return ServiceResponse<JourneyDto>.Return409("Invalid Journey Id");
            var previousHours = journey.Hours.GetValueOrDefault();

            journey.Hours = hours;
            journey.EndDate = journey.StartDate!.Value.AddHours(hours);
            _context.Entry(journey).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await _logger.LogInformation(new FMSLogs()
            {
                Description = $"Journey Hours Changed from: {previousHours} to {journey.Hours}",
                JobNum = journey.Id,
                OrderNum = (int)journey.OrderNum!,
                Location = journey.StartLocation,
                Status = journey.Status,
                LogType = LogTypeEnum.Update.ToString(),
            });

            var response = _mapper.Map<JourneyDto>(journey);
            return ServiceResponse<JourneyDto>.ReturnResultWith201(response);
        }

        public async Task<ServiceResponse<JourneyDto>> UpdateJourney(JourneyDto model)
        {
            var journey = await _context.Journeys.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (journey == null) return ServiceResponse<JourneyDto>.Return409("Invalid Journey Id");
            _mapper.Map<Journey>(model);
            _context.Journeys.Entry(journey).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            var response = _mapper.Map<JourneyDto>(journey);
            await _logger.LogInformation(new FMSLogs()
            {
                Description = $"Journey: {journey.Id} Data Update ",
                JobNum = journey.Id,
                OrderNum = (int)journey.OrderNum!,
                Location = journey.StartLocation,
                Status = journey.Status,
                LogType = LogTypeEnum.Update.ToString(),
            });
            return ServiceResponse<JourneyDto>.ReturnResultWith201(response);
        }

        public async Task<ServiceResponse<JourneyDto>> UpdateJourneyTime(JourneyTimeUpdateDto model)
        {
            var journey = await _context.Journeys.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (journey == null) return ServiceResponse<JourneyDto>.Return409("Invalid Journey Id");
            var prevHours = journey.Hours.GetValueOrDefault();

            journey.TravelStartTime = model.TravelStartTime;
            journey.TravelEndTime = model.TravelEndTime;
            journey.Hours = (int) model.TravelEndTime.Subtract(model.TravelStartTime).TotalHours;

            _context.Journeys.Entry(journey).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            var response = _mapper.Map<JourneyDto>(journey);
            await _logger.LogInformation(new FMSLogs()
            {
                Description = $"Journey Hours Updated from : {prevHours} To {journey.Hours}",
                JobNum = journey.Id,
                OrderNum = (int)journey.OrderNum!,
                Location = journey.StartLocation,
                Status = journey.Status,
                LogType = LogTypeEnum.Update.ToString(),
            });
            return ServiceResponse<JourneyDto>.ReturnResultWith201(response);
        }

        public async Task<ServiceResponse<JourneyDto>> DeleteJourney(int id)
        {
            var journey = await _context.Journeys.FirstOrDefaultAsync(x => x.Id == id);
            if (journey == null) return ServiceResponse<JourneyDto>.Return409("Invalid Journey Id");
            _context.Entry(journey).State = EntityState.Deleted;
            await _context.SaveChangesAsync();
            _mapper.Map<JourneyDto>(journey);
            await _logger.LogInformation(new FMSLogs()
            {
                Description = $"Journey {journey.Id} Deleted Successfully",
                JobNum = journey.Id,
                OrderNum = (int)journey.OrderNum!,
                Location = journey.StartLocation,
                Status = journey.Status,
                LogType = LogTypeEnum.Delete.ToString(),
            });
            return ServiceResponse<JourneyDto>.ReturnSuccess();
        }

        public async Task<ServiceResponse<JourneyDto>> DeleteJourneyDetail(int detailId)
        {
            var journeyDetail = await _context.JourneyDetails.FirstOrDefaultAsync(x => x.Id == detailId);
            if (journeyDetail == null) return ServiceResponse<JourneyDto>.Return409("Invalid Journey Id");
            _context.Entry(journeyDetail).State = EntityState.Deleted;
            await _context.SaveChangesAsync();
            _mapper.Map<JourneyDetail>(journeyDetail);
            await _logger.LogInformation(new FMSLogs()
            {
                Description = $"Journey Detail: {journeyDetail.Id}  Deleted Successfully",
                //JobNum = journey.Id,
                //OrderNum = (int)journey.OrderNum!,
                Location = journeyDetail.Location,
                //Status = journey.Status,
                LogType = LogTypeEnum.Delete.ToString(),
            });
            return ServiceResponse<JourneyDto>.ReturnSuccess();
        }

        public async Task<ServiceResponse<JourneyDto>> CreateJourneyFromOrderNumber(string company,decimal orderNumber)
        {
            var orderResponse = await _orderService.GetOrderByNumber(company, orderNumber);
            if (orderResponse is not { Success: true, Data: not null })
                return ServiceResponse<JourneyDto>.ReturnFailed(orderResponse.StatusCode, orderResponse.Errors);
            var order = orderResponse.Data;
            if (order.OrderDtls == null)
                return ServiceResponse<JourneyDto>.ReturnFailed(orderResponse.StatusCode, orderResponse.Errors);
            var partDetails = order.OrderDtls.FirstOrDefault();
            if (partDetails == null) return ServiceResponse<JourneyDto>.Return422("Part Information does not exists");
            var location = await HelperMethods.GetLongLatInStringByAddress(order.ShipToAddressFormatted!);
            var job = new Journey()
            {
                OrderNum = order.OrderNum,
                Company = order.Company,
                CustNum = order.CustNum,
                CustomerId = order.CustomerCustID,
                CustomerName = order.CustomerName,
                OrderDate = order.OrderDate,
                PartNum = partDetails.PartNum,
                VehicleNum = partDetails.PartNum,
                StartDate = order.RequestDate,
                TravelStartTime = order.RequestDate.GetValueOrDefault(),
                TravelEndTime = order.RequestDate.GetValueOrDefault().AddHours((int)partDetails.OrderQty.GetValueOrDefault()),
                EndDate = order.RequestDate.GetValueOrDefault().AddHours((int)partDetails.OrderQty.GetValueOrDefault()),
                Hours = (int)partDetails.OrderQty.GetValueOrDefault(),
                StartLocation = _startLocation,
                EndLocation = location,
                NeedByDate = order.NeedByDate,
                RequestDate = order.RequestDate,
                //JourneyDetails = details
            };

            await _context.Journeys.AddAsync(job);
            var truck = await _context.Trucks.FirstOrDefaultAsync(x => x.Part_PartNum == partDetails.PartNum);
            if (truck != null)
            {
                truck.CurrentLocation = _startLocation;
                _context.Trucks.Update(truck);
            }
            await _context.SaveChangesAsync();
                    
            await _logger.LogInformation(new FMSLogs()
            {
                Description = $"Journey Created From Order: {order.OrderNum} with Journey Id: {job.Id}",
                JobNum = job.Id,
                OrderNum = (int)job.OrderNum!,
                Location = job.StartLocation,
                Status = job.Status,
                LogType = LogTypeEnum.Update.ToString(),
            });

            return ServiceResponse<JourneyDto>.ReturnResultWith201(_mapper.Map<JourneyDto>(job));
        }

        public async Task<ServiceResponse<string>> DeleteJourney(decimal orderNumber)
        {
            var journey = await _context.Journeys.FirstOrDefaultAsync(x => x.OrderNum == orderNumber);
            if (journey == null) ServiceResponse<string>.Return422("Journey Does Not Found");

            _context.Journeys.Entry(journey!).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            await _logger.LogInformation(new FMSLogs()
            {
                Description = $"Journey {journey!.Id} Deleted ",
                JobNum = journey.Id,
                OrderNum = (int)journey.OrderNum!,
                Location = journey.StartLocation,
                Status = journey.Status,
                LogType = LogTypeEnum.Delete.ToString(),
            });
            return ServiceResponse<string>.ReturnResultWith200("Record Deleted Successfully");
        }

        public async Task<PaginationResponse<JourneyDto>> GetList(JourneyFilterParams filter)
        {
            var query = _context.Journeys.AsQueryable();

            if (!string.IsNullOrEmpty(filter.CustomerName))
            {
                // trim & ignore casing
                var searchQueryForWhereClause = filter.CustomerName.Trim().ToLowerInvariant();
                query = query.Where(a =>
                    a.CustomerName != null && EF.Functions.Like(a.CustomerName, $"{searchQueryForWhereClause}%"));
            }
            if (!string.IsNullOrEmpty(filter.VehicleNum))
            {
                // trim & ignore casing
                query = query.Where(a => (a.VehicleNum != null && a.VehicleNum == filter.VehicleNum));
            }
            if (filter.OrderNum != null)
            {
                query = query.Where(a => a.OrderNum == filter.OrderNum);
            }
            if (filter.StartDate != null)
            {
                query = query.Where(a => a.StartDate!.Value.Date == filter.StartDate.GetValueOrDefault().Date);
            }
            if (!string.IsNullOrEmpty(filter.Filter))
            {
                // trim & ignore casing
                var searchQueryForWhereClause = filter.Filter.Trim().ToLowerInvariant();
                query = query.Where(a =>
                    (a.CustomerName != null && EF.Functions.Like(a.CustomerName, $"{searchQueryForWhereClause}%"))
                    || EF.Functions.Like(a.VehicleNum!, $"%{searchQueryForWhereClause}%")
                    || EF.Functions.Like(a.CustomerId!, $"%{searchQueryForWhereClause}%")
                    || (filter.OrderNum != null && a.OrderNum == filter.OrderNum)
                    );
            }
            // Ordering based on the "orderBy" query parameter
            query = filter.OrderBy!.ToLower() switch
            {
                "ordernum" => query.OrderBy(item => item.OrderNum),
                _ => query.OrderBy(item => item.Id)
            };
            var totalItems = query.Count();
            var items = await query.Skip(filter.Skip!.Value).Take(filter.Top!.Value).ToListAsync();
            var result = _mapper.Map<List<JourneyDto>>(items);
            return new PaginationResponse<JourneyDto>()
            {
                CurrentPage = filter.Skip.GetValueOrDefault(),
                Items = result,
                TotalItems = totalItems,
                PageSize = filter.Skip.GetValueOrDefault()
            };
        }

        public async Task<ServiceResponse<List<VehicleDto>>> AssignJobsToTrucks(List<KFmsOrder> ordersList)
        {
            var truckListResponse = await _trucksService.GetTrucksList(new FilterParams());
            if (truckListResponse is not { Success: true, Data.value: not null })
                return ServiceResponse<List<VehicleDto>>.ReturnFailed(truckListResponse.StatusCode,
                    truckListResponse.Errors);
            var trucks = truckListResponse.Data.value!.Select(x => new VehicleDto()
            {
                PartNum = x.Part_PartNum,
                Description = x.Part_PartDescription,
                Journeys = new List<Journey>()
            }).ToList();

            var i = 0;
            var assignedHours = 0;
            DateTime? lastOrderEndDate = null;
            var jobsStartTime = default(DateTime);
            var startLocation = _startLocation;
            if (ordersList.Any())
            {
                jobsStartTime =
                    DateTime.Parse(
                        ordersList.FirstOrDefault()!.OrderHed_NeedByDate.GetValueOrDefault().ToShortDateString() +
                        " 08:00 AM");
                ordersList.FirstOrDefault()!.OrderHed_NeedByDate = jobsStartTime;
                //startLocation = START_LOCATION;
            }
                
            foreach (var order in ordersList)
            {
                nextTruck:
                var availableTruck = trucks.Skip(i).Take(1).FirstOrDefault();
                //var availableTruck = trucks.FirstOrDefault(truck => CalculateTotalAssignedHours(truck) + order.OrderDtl_OrderQty <= VEHICLE_HOURS);
                if (availableTruck == null) continue;
                // ReSharper disable once AccessToModifiedClosure
                var availableJourneys = await _context.Journeys.Where(x =>
                    x.StartDate!.Value.Date == order.OrderHed_NeedByDate.GetValueOrDefault().Date &&
                    x.VehicleNum == availableTruck.PartNum).ToListAsync();
                if (availableJourneys.Any())
                {
                    if (availableJourneys.Any(x => x.OrderNum == order.OrderHed_OrderNum))
                        continue; //skip if order num already exists

                    assignedHours = availableJourneys.Sum(x => x.Hours).GetValueOrDefault();
                    order.OrderHed_NeedByDate = availableJourneys.LastOrDefault()!.EndDate;
                }
                //If Truck Hours Limit is exists
                if (assignedHours + order.OrderDtl_OrderQty <= _vehicleHours)
                {
                    if (lastOrderEndDate != null)
                    {
                        //Next Order StartDate will be lastEnded Date, to safe dates overlap adding 30 sec.. 
                        order.OrderHed_NeedByDate = lastOrderEndDate.Value.AddSeconds(30);
                    }
                    try
                    {
                        var location = await HelperMethods.GetLongLatInStringByAddress(order.Customer_Address1!);
                        var journey = new Journey()
                        {
                            OrderNum = order.OrderHed_OrderNum,
                            Company = order.OrderHed_Company,
                            CustNum = order.Customer_CustNum,
                            CustomerId = order.Customer_CustID,
                            CustomerName = order.Customer_Name,
                            OrderDate = order.OrderHed_OrderDate,
                            PartNum = order.OrderDtl_PartNum,
                            VehicleNum = availableTruck.PartNum,
                            StartDate = order.OrderHed_NeedByDate,
                            TravelStartTime = order.OrderHed_NeedByDate,
                            TravelEndTime = order.OrderHed_NeedByDate.GetValueOrDefault().AddHours(order.OrderDtl_OrderQty),
                            EndDate = order.OrderHed_NeedByDate.GetValueOrDefault().AddHours(order.OrderDtl_OrderQty),
                            StartLocation = startLocation,
                            Hours = (int)order.OrderDtl_OrderQty,
                            EndLocation = location, 
                            NeedByDate = order.OrderHed_NeedByDate,
                            RequestDate = order.OrderHed_RequestDate,
                            CustomerAddress = order.Customer_Address1,
                            PONum = order.OrderHed_PONum,
                            EntryPerson = order.OrderHed_EntryPerson,
                            Status = "ASSIGNED",
                            OrderHed_SysRowId = order.OrderHed_SysRowID,
                            //JourneyDetails = details No Stops added from Epicor
                        };
                            
                        await _context.Journeys.AddAsync(journey);
                        var truck = await _context.Trucks.FirstOrDefaultAsync(x => x.Part_PartNum == availableTruck.PartNum);
                        if (truck != null)
                        {
                            truck.CurrentLocation = startLocation;
                            _context.Trucks.Update(truck);
                        }
                        if (await _context.SaveChangesAsync()> 0)
                        {
                            availableTruck.Journeys!.Add(journey);
                            lastOrderEndDate = journey.EndDate;
                            startLocation = journey.EndLocation;

                            var description =
                                $"Journey is Created by id {journey.Id} and Assigned to  {journey.VehicleNum} on Date {journey.StartDate}";
                            await _logger.LogInformation(new FMSLogs()
                            {
                                Description = description,
                                JobNum = journey.Id,
                                OrderNum = (int)journey.OrderNum!,
                                Location = journey.StartLocation,
                                Status = journey.Status,
                                LogType = LogTypeEnum.Create.ToString(),
                            });
                            await _logger.LogUd105(journey.Company!, journey.OrderNum.GetValueOrDefault(),
                                journey.VehicleNum!, description, journey.StartLocation, journey.EndLocation,
                                journey.Status!, journey.Id, journey.CurrentLocation ?? "");
                        }
                    }
                    catch(Exception ex)
                    {
                        await _logger.LogExceptionAsync(ex);
                    }
                }
                //when truck limit exceeds iterate to next truck
                else
                {
                    i += 1;
                    assignedHours = 0;
                    lastOrderEndDate = null; //reset order end date

                    //order.OrderHed_OrderDate = order.OrderHed_OrderDate.GetValueOrDefault().
                    order.OrderHed_NeedByDate = jobsStartTime;
                    startLocation = _startLocation;
                    goto nextTruck;
                }

            }
            return ServiceResponse<List<VehicleDto>>.ReturnResultWith200(trucks);

        }

        public async Task<ServiceResponse<object>> GetDriverCommissionDetail(string truckName, DateTime date)
        {
            try
            {
                var truckJourneys = await _context.Journeys
                    .Where(c => c.NeedByDate != null && c.VehicleNum == truckName && c.Status == "COMPLETED" &&
                                c.NeedByDate.Value.Date == date.ToLocalTime().Date).ToListAsync();
                var calcHours = 0;
                var calcCommission = 0.00;
                var calcOverTime = 0;
                foreach (var truckJourney in truckJourneys)
                {
                    calcCommission += 20;
                    calcHours += truckJourney.Hours ?? 0;
                    calcOverTime += Math.Abs((truckJourney.ActualHours ?? 0) - (truckJourney.Hours ?? 0));
                }

                var driverCommissionDetails = new
                {
                    TruckName = truckName,
                    CalcHours = calcHours,
                    CalcCommission = calcCommission,
                    CalcOverTime = calcOverTime,
                };
                
                var truckJourneysHistory = await _context.Journeys
                    .Where(c => c.NeedByDate != null && c.VehicleNum == truckName &&
                                c.NeedByDate.Value.Date == date.ToLocalTime().Date).ToListAsync();
                var truck = await _context.Trucks.FirstOrDefaultAsync(x => x.Part_PartNum == truckName);
                var result = new
                {
                    CommissionDetails = driverCommissionDetails,
                    History = truckJourneysHistory,
                    TruckLocation = new
                    {
                        truck?.Status,
                        VehicleName = truck?.Part_PartNum,
                        VehicleDescription = truck?.Part_PartDescription,
                        truck?.CurrentLocation
                    },
                };
                return ServiceResponse<object>.ReturnResultWith200(result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.ReturnException(ex);
            }
        }

        public async Task<ServiceResponse<string>> DownloadShipmentReport(int packNum,string fileUrl)
        {
            var reportResponse = await _orderService.GetShipmentReportBytesString(packNum);
            if (reportResponse is { Success: false, Data: null }) return ServiceResponse<string>.Return422("Not Found");

            var bytesString = reportResponse.Data!.output;
            if(bytesString == null) return ServiceResponse<string>.Return422("Invalid bytes array");
            
            var fileName = $"Pack_{packNum}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            var resultUrl =await _fileService.SaveReport(fileName, bytesString,fileUrl);
            return ServiceResponse<string>.ReturnResultWith200(resultUrl);
        }

        #region EPICOR END POINTS

        private async Task<ServiceResponse<SalesOrderResponse>> CreateSalesOrder(SalesOrder model)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
                return ServiceResponse<SalesOrderResponse>.Return422("Missing API Keys in App Settings");

            var request = new RestRequest($"Erp.BO.SalesOrderSvc/SalesOrders")
            {
                Timeout = -1
            };
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");

            var body = JsonSerializer.Serialize(model);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await _restClient.ExecutePostAsync<SalesOrderResponse>(request);
            if (response is { IsSuccessful: true, Data: not null })
                return ServiceResponse<SalesOrderResponse>.ReturnResultWith201(response.Data);

            if(model.UseOTS && response.Content!.Contains("One Time Ship"))
                return ServiceResponse<SalesOrderResponse>.Return422("One Time Shipment Not Enable for the Customer");

            return ServiceResponse<SalesOrderResponse>.ReturnFailed((int)response.StatusCode, response.Content ?? "Failed");
        }

        public async Task<ServiceResponse<SalesOrder>> GetJobByJobNumber(SalesOrder model, string company,
            int jobNumber, FilterParams? filters = null)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
                return ServiceResponse<SalesOrder>.Return422("Missing API Keys in App Settings");

            var request = new RestRequest($"Erp.BO.SalesOrderSvc/SalesOrders('{company}',{jobNumber})")
                {
                    Timeout = -1
                };
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);

            request.AddHeader("Content-Type", "application/json");
            if (filters != null)
            {
                if (!string.IsNullOrWhiteSpace(filters.Expand)) request.AddQueryParameter("$expand", filters.Expand);
                if (!string.IsNullOrWhiteSpace(filters.Filter)) request.AddQueryParameter("$filter", filters.Filter);
                if (!string.IsNullOrWhiteSpace(filters.OrderBy)) request.AddQueryParameter("$select", filters.Select);
            }

            var body = JsonSerializer.Serialize(model);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await _restClient.ExecutePostAsync<SalesOrder>(request);
            return response is { IsSuccessful: true, Data: not null }
                ? ServiceResponse<SalesOrder>.ReturnResultWith201(response.Data)
                : ServiceResponse<SalesOrder>.ReturnFailed((int)response.StatusCode, response.Content ?? "Failed");
        }

        private async Task<ServiceResponse<KListResponse<TrucksDto>>> GetTrucksList(FilterParams filters)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
                return ServiceResponse<KListResponse<TrucksDto>>.Return409("{\"message\":\"Missing API Keys in App Settings\"}");


            var request = new RestRequest("BaqSvc/K_DEM_MOVERS/Data")
            {
                Timeout = -1
            };
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

            var fullUrl = _restClient.BuildUri(request);
            Console.WriteLine("Final URL: " + fullUrl);

            var response = await _restClient.ExecuteGetAsync<KListResponse<TrucksDto>>(request);
            return response is { IsSuccessful: true, Data: not null }
                ? ServiceResponse<KListResponse<TrucksDto>>.ReturnResultWith200(response.Data)
                : ServiceResponse<KListResponse<TrucksDto>>.ReturnFailed((int)response.StatusCode,
                    response.ErrorMessage ?? "Failed");
        }

        public async Task<ServiceResponse<string>> UpdateJobStatus(int id, string status, int hoursWorked,
            string latitude, string longitude)
        {
            var journey = await _context.Journeys.FirstOrDefaultAsync(c => c.Id == id);
            if (journey == null) return ServiceResponse<string>.Return404("Record not found");
            try
            {
                var prevJobStatus = journey.Status!;
                var description = $"Job Status Update From {prevJobStatus} to  {status}";
                if (status == "COMPLETED")
                {
                    var res = await UpdateOrderStatusAndCreateShipment(journey.Company ?? "10GAL",
                        (int)journey.OrderNum.GetValueOrDefault(), status, journey.VehicleNum!, hoursWorked);
                    if (!res.Success) return ServiceResponse<string>.ReturnFailed(res.StatusCode, res.Errors);

                    journey.PartNum = journey.VehicleNum; //Sync Part num from Epicor DB
                    journey.PackNum = res.Data!.PackNum;

                    description =
                        $"Job Status Update From {prevJobStatus} to  {status}, and Shipment Created with PackNum {journey.PackNum}";
                }

                journey.Status = status;
                journey.ActualHours = hoursWorked;
                journey.CurrentLocation = !string.IsNullOrEmpty(latitude) ? latitude + "~" + longitude : null;
                _context.Journeys.Entry(journey).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                var truck = await _context.Trucks.FirstOrDefaultAsync(x => x.Part_PartNum == journey.VehicleNum);
                if(truck != null)
                {
                    truck.CurrentLocation = journey.CurrentLocation;
                    _context.Trucks.Entry(truck).State = EntityState.Modified;
                    _context.Trucks.Update(truck);
                    await _context.SaveChangesAsync();
                }
                await _logger.LogInformation(new FMSLogs()
                {
                    Description = description,
                    JobNum = journey.Id,
                    OrderNum = (int)journey.OrderNum!,
                    PackNum = journey.PackNum.GetValueOrDefault(),
                    Location = journey.CurrentLocation ?? journey.StartLocation,
                    Status = journey.Status,
                    LogType = LogTypeEnum.Update.ToString(),
                });
                await _logger.LogUd105(journey.Company!, journey.OrderNum.GetValueOrDefault(), journey.VehicleNum!,
                    description, journey.StartLocation!, journey.EndLocation!, journey.Status!, journey.Id,
                    journey.CurrentLocation ?? "");

                return ServiceResponse<string>.ReturnResultWith200("Status Updated");
            }
            catch(Exception ex)
            {
                await _logger.LogExceptionAsync(ex);
            }
            return ServiceResponse<string>.Return404("Record not found");
        }

        public async Task<ServiceResponse<string>> UpdateSubJobStatus(int id, string status, int hoursWorked,
            string latitude, string longitude, int? actualHours)
        {
            var journey = await _context.JourneyTrucks.Include(c => c.Journey).FirstOrDefaultAsync(c => c.Id == id);
            if (journey == null) return ServiceResponse<string>.Return404("Record not found");
            try
            {
                journey.Status = status;
                journey.Hours = hoursWorked;
                journey.ActualHours = actualHours ?? 0;
                journey.EndLocation = !string.IsNullOrEmpty(latitude) ? latitude + "~" + longitude : null;
                _context.JourneyTrucks.Entry(journey).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                var truck = await _context.Trucks.FirstOrDefaultAsync(x => x.Part_PartNum == journey.TruckId);
                if (truck == null) return ServiceResponse<string>.ReturnResultWith200("Status Updated");
                truck.CurrentLocation = journey.EndLocation;
                _context.Trucks.Entry(truck).State = EntityState.Modified;
                _context.Trucks.Update(truck);
                await _context.SaveChangesAsync();
                return ServiceResponse<string>.ReturnResultWith200("Status Updated");
            }
            catch(Exception ex)
            {
                await _logger.LogExceptionAsync(ex);
            }
            return ServiceResponse<string>.Return404("Record not found");
        }
        public async Task<ServiceResponse<dynamic>> UpdateExt(Journey journey)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
                return ServiceResponse<dynamic>.Return409("{\"message\":\"Missing API Keys in App Settings\"}");
            if (journey.RowMod == "U")
            {
                journey.RowMod = "";
                _context.Journeys.Entry(journey).State = EntityState.Modified;
                await _context.SaveChangesAsync();    
            }
            if (journey.RowMod == "D")
            {
                if (journey.JourneyTrucks is {Count: > 0})
                    foreach (var journeyTruck in journey.JourneyTrucks)
                    {
                        if (journeyTruck.TruckLocations is { Count: > 0 })
                        {
                            _context.TruckLocations.RemoveRange(journeyTruck.TruckLocations);
                            await _context.SaveChangesAsync();
                        }
                        
                        var dbJourneyTrucks =
                            await _context.JourneyTrucks.FirstOrDefaultAsync(c => c.JourneyId == journey.Id);
                        if (dbJourneyTrucks == null) continue;
                        _context.JourneyTrucks.Remove(dbJourneyTrucks);
                        await _context.SaveChangesAsync();
                    }

                if (journey.JourneyDetails is { Count: > 0 })
                {
                    _context.JourneyDetails.RemoveRange(journey.JourneyDetails);
                    await _context.SaveChangesAsync();
                }
                var dbJourney = await _context.Journeys.FirstOrDefaultAsync(c => c.Id == journey.Id);
                if (dbJourney == null)
                    return ServiceResponse<dynamic>.ReturnResultWith200(new { message = "Success", data = journey });
                _context.Journeys.Remove(dbJourney);
                await _context.SaveChangesAsync();
            }
            else if (journey.JourneyTrucks is { Count: > 0 })
            {
                var journeyTrucksUpdate = journey.JourneyTrucks.Where(c => c.RowMod is "U" or "").ToList();
                foreach (var journeyTruck in journeyTrucksUpdate)
                {
                    journeyTruck.RowMod = "";
                    _context.JourneyTrucks.Entry(journeyTruck).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    if (journeyTruck.TruckLocations == null) continue;
                    var journeyTruckLocations = 
                        journeyTruck.TruckLocations.Where(c => c.RowMod == "U").ToList();
                    foreach (var journeyTruckLocation in journeyTruckLocations)
                    {
                        journeyTruckLocation.RowMod = "";
                        _context.TruckLocations.Entry(journeyTruckLocation).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                    journeyTruckLocations = journeyTruck.TruckLocations.Where(c => c.RowMod == "A").ToList();
                    foreach (var journeyTruckLocation in journeyTruckLocations)
                    {
                        journeyTruckLocation.RowMod = "";
                        await _context.TruckLocations.AddAsync(journeyTruckLocation);
                        await _context.SaveChangesAsync();
                    }
                    journeyTruckLocations = journeyTruck.TruckLocations.Where(c => c.RowMod == "D").ToList();
                    if (journeyTruckLocations is not { Count: > 0 }) continue;
                    _context.TruckLocations.RemoveRange(journeyTruckLocations);
                    await _context.SaveChangesAsync();
                }
                journeyTrucksUpdate = journey.JourneyTrucks.Where(c => c.RowMod == "A").ToList();
                foreach (var journeyTruck in journeyTrucksUpdate)
                {
                    journeyTruck.RowMod = "";
                    await _context.JourneyTrucks.AddAsync(journeyTruck);
                    await _context.SaveChangesAsync();
                    if (journeyTruck.TruckLocations == null) continue;
                    var journeyTruckLocations = journeyTruck.TruckLocations.ToList();
                    foreach (var journeyTruckLocation in journeyTruckLocations)
                    {
                        journeyTruckLocation.RowMod = "";
                        journeyTruckLocation.JourneyTruckId = journeyTruck.Id;
                        await _context.TruckLocations.AddAsync(journeyTruckLocation);
                        await _context.SaveChangesAsync();
                    }
                }
                journeyTrucksUpdate = journey.JourneyTrucks.Where(c => c.RowMod == "D").ToList();
                foreach (var journeyTruck in journeyTrucksUpdate)
                {
                    if (journeyTruck.TruckLocations != null)
                    {
                        _context.TruckLocations.RemoveRange(journeyTruck.TruckLocations);
                        await _context.SaveChangesAsync();
                    }
                    var dbJourneyTruck = await _context.JourneyTrucks.FirstOrDefaultAsync(c => c.Id == journeyTruck.Id);
                    if (dbJourneyTruck == null) continue;
                    _context.JourneyTrucks.Remove(dbJourneyTruck);
                    await _context.SaveChangesAsync();
                }
            }
            else if (journey.JourneyDetails is { Count: > 0 })
            {
                var journeyDetails = journey.JourneyDetails.Where(c => c.RowMod is "U" or "").ToList();
                foreach (var journeyDetail in journeyDetails)
                {
                    journeyDetail.RowMod = "";
                    _context.JourneyDetails.Entry(journeyDetail).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                journeyDetails = journey.JourneyDetails.Where(c => c.RowMod == "A").ToList();
                foreach (var journeyDetail in journeyDetails)
                {
                    journeyDetail.RowMod = "";
                    await _context.JourneyDetails.AddAsync(journeyDetail);
                    await _context.SaveChangesAsync();
                }
                journeyDetails = journey.JourneyDetails.Where(c => c.RowMod == "D").ToList();
                foreach (var journeyDetail in journeyDetails)
                {
                    var dbJourneyDetail = await _context.JourneyDetails.FirstOrDefaultAsync(c => c.Id == journeyDetail.Id);
                    if (dbJourneyDetail == null) continue;
                    _context.JourneyDetails.Remove(dbJourneyDetail);
                    await _context.SaveChangesAsync();
                }
            }

            return ServiceResponse<dynamic>.ReturnResultWith200(new {message = "Success", data = journey});
        }
        public async Task<ServiceResponse<List<TruckStatsDto>>> GetTruckStats(DateTime startDate, DateTime endDate)
        {
            // Get all trucks from the database in one call
            var trucks = await _context.Trucks.AsNoTracking().ToListAsync();

            // Get all relevant, completed journeys for ALL trucks in a single database call
            var relevantJourneys = await _context.Journeys
                .Where(j => j.Status == "COMPLETED" && j.StartDate.Value.Date >= startDate.Date && j.EndDate.Value.Date <= endDate.Date)
                .ToListAsync();

            // Create a list to hold the final statistics
            var statsList = new List<TruckStatsDto>();

            // Now, process the data in memory without calling the database in a loop
            foreach (var truck in trucks)
            {
                var truckStats = new TruckStatsDto
                {
                    PartNum = truck.Part_PartNum,
                    Description = truck.Part_PartDescription,
                    TotalRevenue = 0,
                    TotalDistance = 0
                };

                // Filter the journeys for the current truck from the list we already fetched
                var journeysForThisTruck = relevantJourneys.Where(j => j.VehicleNum == truck.Part_PartNum);

                foreach (var journey in journeysForThisTruck)
                {
                    // The external API call to get revenue still happens here for each journey
                    var orderResponse = await _orderService.GetOrderByNumber(journey.Company ?? "10GAL", journey.OrderNum.GetValueOrDefault());
                    if (orderResponse.Success && orderResponse.Data != null)
                    {
                        truckStats.TotalRevenue += orderResponse.Data.OrderAmt.GetValueOrDefault();
                    }

                    // The distance calculation is fast and happens in memory
                    if (!string.IsNullOrEmpty(journey.StartLocation) && !string.IsNullOrEmpty(journey.EndLocation))
                    {
                        truckStats.TotalDistance += HelperMethods.GetDistanceBetweenPoints(journey.StartLocation, journey.EndLocation);
                    }
                }
                statsList.Add(truckStats);
            }

            return ServiceResponse<List<TruckStatsDto>>.ReturnResultWith200(statsList);
        }

        #endregion

        #region private methods
        private async Task<ServiceResponse<KShipmentResponse>> UpdateOrderStatusAndCreateShipment(string company,
            int orderNum, string status, string partNum, int hoursWorked)
        {
            var orderResponse = await _orderService.UpdateOrderStatus(company, orderNum, status, partNum,hoursWorked);
            if (orderResponse is { Success: false, Data: null })
                return ServiceResponse<KShipmentResponse>.ReturnFailed(orderResponse.StatusCode, orderResponse.Errors);

            var order = orderResponse.Data;
            var orderDtl = order!.OrderDtls!.FirstOrDefault();
            //Apply Customer Shipment
            var shipmentResponse = await _orderService.CreateShipment(new KCreateShipmentDto()
            {
                Company = order.Company,
                PackNum = 0,
                ShipDate = order.OrderDate,
                TrackingNumber = string.Empty,
                ShipStatus = "OPEN",
                ChrgAmount = order.DocOrderAmt,
                OTSOrderNum = order.OrderNum,
                BTCustNum = order.BTCustNum,
                CustNum = order.CustNum,
                BTCustID = order.BTCustID,
                CurrencyCode = order.CurrencyCode,
                ShipToCustNum = order.ShipToCustNum,
                ServRef1 = "1",
                ShipDtls = new List<ShipDtlCreateDtp>()
                {
                    new ShipDtlCreateDtp()
                    {
                        Company = order.Company,
                        PackNum = 0,
                        PackLine  = 0,
                        OrderNum = order.OrderNum,
                        OrderLine = 1,
                        OrderRelNum = 1,
                        LineType = "PART",
                        OurInventoryShipQty = hoursWorked,
                        Packages = 1,
                        PartNum = orderDtl!.PartNum,
                        LineDesc = orderDtl.LineDesc,
                        IUM = "EA",
                        ShipCmpl = true,
                        WarehouseCode="OSB",
                        BinNum = "601013",
                        CustNum =  order.CustNum,
                        InventoryShipUOM = "EA",
                        PackNumShipStatus = "OPEN",
                        //Plant: order.Plant,
                        ReadyToInvoice = false,
                        SellingInventoryShipQty = orderDtl.SellingQuantity,
                        SellingFactor = 1,
                        SalesUM = "EA",
                        SellingFactorDirection = "D",
                        //BinType = "Std",
                        ShipToCustNum = order.ShipToCustNum,
                        //InDiscount = 0,
                        MFCustNum = order.ShipToCustNum,
                        MFShipToNum = order.ShipToNum,
                        DisplayInvQty = hoursWorked,
                        //KitQtyFromInv =  0,
                        MarkForAddrList = order.ShipToAddressList,
                        MFCustID = order.ShipToCustId,
                        OrderHold = false,
                        OurReqUM = "EA",
                        //PartUseHTSDesc = false,
                        RequestDate = order.OrderDate,
                        CustNumCustID = order.ShipToCustId,
                        OrderNumPSCurrCode = order.CurrencyCode,
                        OrderNumCurrencyCode = order.CurrencyCode,
                        CurrencyCode = order.CurrencyCode,
                        OrderLineProdCode = "FABP",
                        PartNumPricePerCode = "E",
                        PartNumSellingFactor = 1,
                        PartNumSalesUM = "EA",
                        PartNumIUM = "EA",
                        LotNum = "",
                        Invoiced = false,
                    }
                }
            });
            return shipmentResponse is { Success: true, Data: not null } ? 
                ServiceResponse<KShipmentResponse>.ReturnResultWith200(shipmentResponse.Data) : 
                ServiceResponse<KShipmentResponse>.ReturnFailed(shipmentResponse.StatusCode, shipmentResponse.Errors);
        }

        public async Task<ServiceResponse<LocationResponse>> GetGoogleLocationDetails(string address)
        {
           return await HelperMethods.GetGoogleLocationDetails(address);
        }

        #endregion
    }

}
