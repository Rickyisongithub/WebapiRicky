using KairosWebAPI.Models.Dto;
using RestSharp;
using Microsoft.Extensions.Logging;  // Add this

namespace KairosWebAPI.Services.EpicorAuthService
{
    public class EpicorAuthService : IEpicorAuthService
    {
        private readonly RestClient _restClient;
        private readonly string _apiKey;
        private readonly string _authorizationKey;
        private readonly ILogger<EpicorAuthService> _logger;  // Add this

        public EpicorAuthService(IConfiguration config, ILogger<EpicorAuthService> logger)  // Add logger parameter
        {
            var configuration = config;
            _restClient = new RestClient(configuration["kairos:API_Efx_Url"] ?? string.Empty);
            _apiKey = configuration["kairos:API_KEY"] ?? string.Empty;
            _authorizationKey = configuration["kairos:AUTHORIZATION_KEY"] ?? string.Empty;
            _logger = logger;  // Initialize logger
        }

        public async Task<EpicorAuthResponseDto> AuthenticateUser(string empId, string password)
        {
            try
            {
                _logger.LogInformation($"EpicorAuthService: Authenticating user {empId}");

                if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
                {
                    _logger.LogError("EpicorAuthService: API configuration missing");
                    return new EpicorAuthResponseDto
                    {
                        isAuthenticated = false,
                        responseMessage = "API configuration missing"
                    };
                }

                var request = new RestRequest("KFMS/UserAuthentication")
                {
                    Timeout = 30000  // 30 seconds timeout
                };

                request.AddHeader("Authorization", "Basic " + _authorizationKey);
                request.AddHeader("X-API-Key", _apiKey);
                request.AddHeader("Content-Type", "application/json");

                var data = new
                {
                    empID = empId,
                    pin = int.TryParse(password, out int pinValue) ? pinValue : 0
                };

                request.AddBody(data);

                _logger.LogInformation($"EpicorAuthService: Sending request to {_restClient.Options.BaseUrl}/KFMS/UserAuthentication");

                var response = await _restClient.ExecutePostAsync<EpicorAuthResponseDto>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    _logger.LogInformation($"EpicorAuthService: Authentication result - {response.Data.isAuthenticated}, Message: {response.Data.responseMessage}");
                    return response.Data;
                }
                else
                {
                    _logger.LogError($"EpicorAuthService: Request failed - Status: {response.StatusCode}, Error: {response.ErrorMessage}");
                    return new EpicorAuthResponseDto
                    {
                        isAuthenticated = false,
                        responseMessage = $"Authentication failed: {response.ErrorMessage ?? "Unknown error"}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EpicorAuthService: Exception during authentication");
                return new EpicorAuthResponseDto
                {
                    isAuthenticated = false,
                    responseMessage = $"Error: {ex.Message}"
                };
            }
        }
    }
}