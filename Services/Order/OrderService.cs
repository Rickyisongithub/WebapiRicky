using System.Text.Json;
using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Models.ResponseResults;
using KairosWebAPI.Models.ResponseResults.SaleOrder;
using KairosWebAPI.Models.ResponseResults.Shipment;
using KairosWebAPI.Services.Trucks;
using RestSharp;

namespace KairosWebAPI.Services.Order
{

    public class OrderService : IOrderService
    {
        private readonly RestClient _restClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _authorizationKey;
        private readonly IHostEnvironment _env;
        private readonly ITrucksService _truckService;

        public OrderService(IConfiguration config, IHostEnvironment env,ITrucksService truckService)
        {

            _configuration = config;
            _restClient = new RestClient(_configuration["kairos:API_Url"] ?? string.Empty);
            _apiKey = _configuration["kairos:API_KEY"] ?? string.Empty;
            _authorizationKey = _configuration["kairos:AUTHORIZATION_KEY"] ?? string.Empty;
            _env = env;
            _truckService = truckService;
        }
        public async Task<ServiceResponse<KListResponse<KFmsOrder>>> GetFmsOrders(FMSOrderParams filters)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
                return ServiceResponse<KListResponse<KFmsOrder>>.Return409("{\"message\":\"Missing API Keys in App Settings\"}");


            var request = new RestRequest("BaqSvc/K_FMS_ORDERS/Data?OrderDate=" + filters.OrderDate.ToString("yyyy-MM-dd"))
                {
                    Timeout = -1
                };
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");

            if (!string.IsNullOrWhiteSpace(filters.Select)) request.AddParameter("$select", filters.Select);
            if (!string.IsNullOrWhiteSpace(filters.Filter)) request.AddQueryParameter("$filter", filters.Filter);
            if (!string.IsNullOrWhiteSpace(filters.OrderBy)) request.AddQueryParameter("$orderBy", filters.OrderBy);
            if (filters.Top.GetValueOrDefault() != default) request.AddQueryParameter("$top", filters.Top.ToString());
            if (filters.Skip.GetValueOrDefault() != default) request.AddQueryParameter("$skip", filters.Skip.ToString());
            if (filters.Count.GetValueOrDefault() != default) request.AddQueryParameter("$count", filters.Count.ToString());

            var response = await _restClient.ExecuteGetAsync<KListResponse<KFmsOrder>>(request);
            return response is { IsSuccessful: true, Data: not null } ? 
                ServiceResponse<KListResponse<KFmsOrder>>.ReturnResultWith200(response.Data) : 
                ServiceResponse<KListResponse<KFmsOrder>>.ReturnFailed((int)response.StatusCode, response.ErrorMessage ?? "Failed");
        }
        public async Task<ServiceResponse<KListResponse<SalesOrderResponse>>> GetOrderList(FilterParams filters)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
                return ServiceResponse<KListResponse<SalesOrderResponse>>.Return409("{\"message\":\"Missing API Keys in App Settings\"}");


            var request = new RestRequest("Erp.BO.SalesOrderSvc/SalesOrders")
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

            var response = await _restClient.ExecuteGetAsync<KListResponse<SalesOrderResponse>>(request);
            if (response.IsSuccessful && response.Data != null)
            {
                return ServiceResponse<KListResponse<SalesOrderResponse>>.ReturnResultWith200(response.Data);

            }

            return ServiceResponse<KListResponse<SalesOrderResponse>>.ReturnFailed((int)response.StatusCode,response.ErrorMessage ?? "Failed");
        }

        public async Task<ServiceResponse<SalesOrderResponse>> GetOrderByNumber(string company,decimal orderNumber)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
                return ServiceResponse<SalesOrderResponse>.Return409("{\"message\":\"Missing API Keys in App Settings\"}");


            var request = new RestRequest($"Erp.BO.SalesOrderSvc/SalesOrders('{company}',{orderNumber})")
                {
                    Timeout = -1
                };
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");
            
            request.AddQueryParameter("$expand", "OrderDtls");

            var response = await _restClient.ExecuteGetAsync<SalesOrderResponse>(request);
            if (response.IsSuccessful && response.Data != null)
            {
                return ServiceResponse<SalesOrderResponse>.ReturnResultWith200(response.Data);

            }

            return ServiceResponse<SalesOrderResponse>.ReturnFailed((int)response.StatusCode, response.ErrorMessage ?? "Failed");
        }

        public async Task<ServiceResponse<AttachmentsResponse<AttachmentsValue>>> GetAttachments(int orderNumber)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
                return ServiceResponse<AttachmentsResponse<AttachmentsValue>>.Return409("{\"message\":\"Missing API Keys in App Settings\"}");


            var request = new RestRequest($"Ice.BO.AttachmentSvc/Attachments?$select=Key1,Company,XFileRefDocTypeID,XFileRefNum,ForeignSysRowID,XFileRefXFileName,XFileRefXFileDesc&$filter=Key1 eq '{orderNumber}'");
            request.Timeout = -1;
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");

            var response = await _restClient.ExecuteGetAsync<AttachmentsResponse<AttachmentsValue>>(request);
            if (response.IsSuccessful && response.Data != null)
            {
                return ServiceResponse<AttachmentsResponse<AttachmentsValue>>.ReturnResultWith200(response.Data);

            }

            return ServiceResponse<AttachmentsResponse<AttachmentsValue>>.ReturnFailed((int)response.StatusCode, response.ErrorMessage ?? "Failed");
        }

        public async Task<ServiceResponse<string>> SaveAttachment(SaveAttachmentDto model, string fileUrl)
        {
            try
            {
                var path = Path.Combine(_env.ContentRootPath, "wwwroot/Files");
                byte[] bytes = Convert.FromBase64String(model.Base64Image!);
                var fileName = $"{Guid.NewGuid()}.png";
                var filePath = Path.Combine(path, fileName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                await File.WriteAllBytesAsync(filePath, bytes);
                var saveAttachmentEpicorDto = new SaveAttachmentEpicorDTO
                {
                    ForeignSysRowID = model.OrderHed_SysRowID,
                    Company = model.Company,
                    RelatedToSchemaName = "Erp",
                    RelatedToFile = "OrderHed",
                    Key1 = model.Key1,
                    XFileRefDocTypeID = model.XFileRefDocTypeID,
                    XFileRefXFileDesc = model.XFileRefXFileDesc,
                    XFileRefXFileName = fileUrl + fileName,
                };

                if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey)) return ServiceResponse<string>.Return422("Missing API Keys in App Settings");

                var request = new RestRequest($"Ice.BO.AttachmentSvc/Attachments");
                request.Timeout = -1;
                request.AddHeader("Authorization", "Basic " + _authorizationKey);
                request.AddHeader("X-API-Key", _apiKey);
                request.AddHeader("Content-Type", "application/json");

                var body = JsonSerializer.Serialize(saveAttachmentEpicorDto);
                request.AddParameter("application/json", body, ParameterType.RequestBody);

                var response = await _restClient.ExecutePostAsync(request);
                if (response.IsSuccessful)
                    return ServiceResponse<string>.ReturnResultWith201(fileUrl + fileName);

                return ServiceResponse<string>.ReturnFailed((int)response.StatusCode, response.Content ?? "Failed");
            }
            catch(Exception ex)
            {
                return ServiceResponse<string>.ReturnException(ex);
            }
        }

        public async Task<ServiceResponse<DocTypeResponse<DocTypeValue>>> GetDocType(string company)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey)) return ServiceResponse<DocTypeResponse<DocTypeValue>>.Return422("Missing API Keys in App Settings");

            var request = new RestRequest($"Ice.BO.DocTypeSvc/DocTypes?$select=DocTypeID,Description&$filter=Company eq '{company}'");
            request.Timeout = -1;
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");


            var response = await _restClient.ExecuteGetAsync<DocTypeResponse<DocTypeValue>>(request);
            if (response.IsSuccessful && response.Data != null)
            {
                return ServiceResponse<DocTypeResponse<DocTypeValue>>.ReturnResultWith200(response.Data);
            }
            return ServiceResponse<DocTypeResponse<DocTypeValue>>.ReturnFailed((int)response.StatusCode, response.ErrorMessage ?? "Failed");
        }

        public async Task<ServiceResponse<string>> UpdateOrder(SalesOrderResponse model, string company, int orderNumber)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey)) return ServiceResponse<string>.Return422("Missing API Keys in App Settings");

            var request = new RestRequest($"Erp.BO.SalesOrderSvc/SalesOrders('{company}',{orderNumber})", Method.Patch);
            request.Timeout = -1;
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");

            var body = JsonSerializer.Serialize(model);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await _restClient.ExecuteAsync(request);
            if (response.IsSuccessful && StatusCodes.Status204NoContent == (int)response.StatusCode)
                return ServiceResponse<string>.ReturnResultWith200(await Task.FromResult("Record Updated Successfully"));

            return ServiceResponse<string>.ReturnFailed((int)response.StatusCode, response.Content ?? "Failed");
        }
        public async Task<ServiceResponse<SalesOrderResponse>> UpdateOrderStatus(string company, int orderNumber, string status, string partNum,int hoursWorked)
        {
            var orderResponse = await GetOrderByNumber(company, orderNumber);
            if (orderResponse.Success && orderResponse.Data != null)
            {
                var order = orderResponse.Data; //Every order has only one order line
                var truckResponse = await _truckService.GetTrucksList(new FilterParams());
                if(!truckResponse.Success && truckResponse.Data == null) return ServiceResponse<SalesOrderResponse>.ReturnFailed(truckResponse.StatusCode, truckResponse.Errors);

                var part = truckResponse.Data!.value!.FirstOrDefault(x => x.Part_PartNum == partNum);
                if (order.OrderDtls != null && part != null)
                {
                    order.OrderStatus = status;
                    order.ShipOrderComplete = true;
                    order.OrderDtls[0].PartNum = part.Part_PartNum;
                    order.OrderDtls[0].LineDesc = part.Part_PartDescription;
                    order.OrderDtls[0].OrderQty = hoursWorked;
                    order.OrderDtls[0].SellingQuantity = hoursWorked;
                }

                var updateOrderResponse = await this.UpdateOrder(order,company,orderNumber);
                if (updateOrderResponse.Success && updateOrderResponse.Data != null)
                    return ServiceResponse<SalesOrderResponse>.ReturnResultWith200(order);
                else
                    return ServiceResponse<SalesOrderResponse>.ReturnFailed(updateOrderResponse.StatusCode,updateOrderResponse.Errors);

            }
            else
            {
                return ServiceResponse<SalesOrderResponse>.ReturnFailed(orderResponse.StatusCode, orderResponse.Errors);
            }
        }

        //Create Customer Shipment
        public async Task<ServiceResponse<KShipmentResponse>> CreateShipment(KCreateShipmentDto model)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey)) return ServiceResponse<KShipmentResponse>.Return422("Missing API Keys in App Settings");

            var request = new RestRequest($"Erp.BO.CustShipSvc/CustShips");
            request.Timeout = -1;
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");

            var body = JsonSerializer.Serialize(model);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await _restClient.ExecutePostAsync<KShipmentResponse>(request);
            if (response.IsSuccessful && response.Data != null)
                return ServiceResponse<KShipmentResponse>.ReturnResultWith201(response.Data);

            return ServiceResponse<KShipmentResponse>.ReturnFailed((int)response.StatusCode, response.Content ?? "Failed");
        }
        public async Task<ServiceResponse<KShipmentReportResponse>> GetShipmentReportBytesString(int packNum)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey)) return ServiceResponse<KShipmentReportResponse>.Return422("Missing API Keys in App Settings");

            RestClient client = new RestClient(_configuration["kairos:API_Efx_Url"] ?? string.Empty);
            var request = new RestRequest("KCustomerShipment/GetShipmentReport");
            request.Timeout = -1;
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");
            var model = new
            {
                PackNum = packNum,
            };
            request.AddParameter("application/json", model, ParameterType.RequestBody);

            var response = await client.ExecutePostAsync<KShipmentReportResponse>(request);
            if (response.IsSuccessful && response.Data != null)
                return ServiceResponse<KShipmentReportResponse>.ReturnResultWith201(response.Data);

            return ServiceResponse<KShipmentReportResponse>.ReturnFailed((int)response.StatusCode, response.Content ?? "Failed");
        }

    }
}