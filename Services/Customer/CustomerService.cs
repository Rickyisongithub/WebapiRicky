using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.FilterDtos;
using KairosWebAPI.Models.ResponseResults;
using RestSharp;

namespace KairosWebAPI.Services.Customer
{
    public class CustomerService:ICustomerService
    {
        private readonly RestClient _restClient;
        private readonly string _apiKey;
        private readonly string _authorizationKey;
        public CustomerService(IConfiguration config)
        {
            _restClient = new RestClient(config["kairos:API_Url"] ?? string.Empty);
            _apiKey = config["kairos:API_KEY"] ?? string.Empty;
            _authorizationKey = config["kairos:AUTHORIZATION_KEY"] ?? string.Empty;
        }

        public async Task<ServiceResponse<KListResponse<CustomerDto>>> GetCustomerList(FilterParams filters)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
                return ServiceResponse<KListResponse<CustomerDto>>.Return409("{\"message\":\"Missing API Keys in App Settings\"}");


            var request = new RestRequest("BaqSvc/K_DEM_CUSTOMERS/Data")
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

            var response = await _restClient.ExecuteGetAsync<KListResponse<CustomerDto>>(request);
            return response is { IsSuccessful: true, Data: not null }
                ? ServiceResponse<KListResponse<CustomerDto>>.ReturnResultWith200(response.Data)
                : ServiceResponse<KListResponse<CustomerDto>>.ReturnFailed((int)response.StatusCode,
                    response.ErrorMessage ?? "Failed");
        }
    }
}

