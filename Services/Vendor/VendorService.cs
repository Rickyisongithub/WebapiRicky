using System.Text.Json;
using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.FilterDtos;
using RestSharp;

namespace KairosWebAPI.Services.Vendor
{
    public class VendorService : IVendorService
    {
        private readonly RestClient _restClient;
        private readonly string _apiKey;
        private readonly string _authorizationKey;

        public VendorService(IConfiguration config)
        {
            var configuration = config;
            _restClient = new RestClient(configuration["kairos:API_Url"] ?? string.Empty);
            _apiKey = configuration["kairos:API_KEY"] ?? string.Empty;
            _authorizationKey = configuration["kairos:AUTHORIZATION_KEY"] ?? string.Empty;

        }

        public async Task<Dictionary<string, string>> GetVendorsList(FilterParams filters)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
            {
                Dictionary<string, string> returnObj = new Dictionary<string, string>();
                returnObj.Add("Status", StatusCodes.Status401Unauthorized.ToString());
                returnObj.Add("Body", "{\"message\":\"Missing API Keys in App Settings\"}");

                return returnObj;
            }


            var request = new RestRequest("Erp.BO.VendorSvc/Vendors");
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

            var response = await _restClient.ExecuteGetAsync(request);
            if (response.IsSuccessful && response.Content != null)
            {
                Dictionary<string, string> returnObj = new Dictionary<string, string>();
                returnObj.Add("Status", response.StatusCode.ToString());
                returnObj.Add("Body", response.Content);
                return returnObj;
            }

            Dictionary<string, string> returnObjError = new Dictionary<string, string>();
            returnObjError.Add("Status", response.StatusCode.ToString());
            returnObjError.Add("Body", "{\"message\":" + response.ErrorMessage! + "}");
            return returnObjError;
        }

        public async Task<ServiceResponse<string>> UpdateVendor(UpdateVendorDto model,string company,int vendorNum)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey)) return ServiceResponse<string>.Return422("Missing API Keys in App Settings");

            var request = new RestRequest($"Erp.BO.VendorSvc/Vendors('{company}',{vendorNum})",Method.Patch);
            request.Timeout = -1;
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");
            
            var body =  JsonSerializer.Serialize(model);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = _restClient.Execute(request);
            if (response.IsSuccessful && StatusCodes.Status200OK == (int)response.StatusCode )
                return ServiceResponse<string>.ReturnResultWith200(await Task.FromResult("Record Updated Successfully"));

            return ServiceResponse<string>.ReturnFailed((int)response.StatusCode, response.Content?? "Failed");
        }
    }
}
