using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.ResponseResults;
using RestSharp;
using Microsoft.Extensions.Logging;  // Add this

namespace KairosWebAPI.Services.KairosService
{
    public class KairosService : IKairosService
    {
        private readonly RestClient _restClient;
        private readonly string _apiKey;
        private readonly string _authorizationKey;
        private readonly ILogger<KairosService> _logger;  // Add this

        public KairosService(IConfiguration config, ILogger<KairosService> logger)  // Add logger parameter
        {
            var configuration = config;
            _restClient = new RestClient(configuration["kairos:API_Url"] ?? string.Empty);
            _apiKey = configuration["kairos:API_KEY"] ?? string.Empty;
            _authorizationKey = configuration["kairos:AUTHORIZATION_KEY"] ?? string.Empty;
            _logger = logger;  // Initialize logger
        }

        public async Task<bool> ValidateUser(LoginDto loginDto)
        {
            _logger.LogInformation($"KairosService: Validating user {loginDto.UserId}");

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey))
            {
                _logger.LogError("KairosService: API Key or Authorization Key is missing");
                return false;
            }

            var request = new RestRequest("Ice.BO.UserFileSvc/ValidatePassword")
            {
                Timeout = 30000  // 30 seconds timeout
            };

            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");

            var data = new
            {
                userID = loginDto.UserId,
                password = loginDto.Password
            };
            request.AddBody(data);

            _logger.LogInformation($"KairosService: Sending request to {_restClient.Options.BaseUrl}/Ice.BO.UserFileSvc/ValidatePassword");

            var response = await _restClient.ExecutePostAsync<KValidateUserResponse>(request);

            if (response.IsSuccessful)
            {
                _logger.LogInformation($"KairosService: Response successful, ReturnObj: {response.Data?.ReturnObj}");
            }
            else
            {
                _logger.LogError($"KairosService: Request failed - Status: {response.StatusCode}, Error: {response.ErrorMessage}");
                _logger.LogError($"KairosService: Response content: {response.Content}");
            }

            return response is { IsSuccessful: true, Data.ReturnObj: true };
        }
    }
}