using System.Globalization;
using KairosWebAPI.DatabaseContext;
using KairosWebAPI.Helpers;
using KairosWebAPI.Models.Entities;
using KairosWebAPI.Models.ResponseResults.UD105Logs;
using RestSharp;
using System.Text.Json;

namespace KairosWebAPI.Services.Logs
{
    public class LogService :ILogService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        private readonly string _authorizationKey;
        private readonly string _apiKey;


        public LogService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _configuration = config;
            _apiKey = _configuration["kairos:API_KEY"] ?? string.Empty;
            _authorizationKey = _configuration["kairos:AUTHORIZATION_KEY"] ?? string.Empty;
        }
        public async Task LogExceptionAsync(Exception ex)
        {
            FMSLogs log = new FMSLogs()
            {
                LogType =LogTypeEnum.Error.ToString(),
                Description = this.ExceptionToMessage(ex),
                Exception = ex.ToString(),
                
            };

            await _context.FMSLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
        public async Task LogInformation(FMSLogs log)
        {
            log.TimeStamp = DateTime.UtcNow;

            await _context.FMSLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }


        private string ExceptionToMessage(Exception ex)
        {
            var exceptionMessage = ex.Message;
            var innerExceptionMessage = ex.InnerException == null ? null : ex.InnerException.Message;
            var stackTrace = ex.StackTrace;
            //var controllerName = ControllerContext RouteData.Values["controller"].ToString();
            //var actionName = FilterContext.RouteData.Values["action"].ToString();
            string logMessege =
                    "Date: " + DateTime.Now.ToString(CultureInfo.InvariantCulture) + Environment.NewLine +
                    //"Controller: " + controllerName + Environment.NewLine +
                    //"Action: " + actionName + Environment.NewLine +
                    "Exception Message: " + exceptionMessage + Environment.NewLine +
                    "Inner Exception Message: " + innerExceptionMessage + Environment.NewLine +
                    "Stack Trace: " + stackTrace;

            return logMessege;
        }

        public async Task<ServiceResponse<string>> LogUd105(string company,decimal orderNum,string partNum,string description,string startLocation,string endLocation, string status,long journeyId, string actualLocation)
        {
            DateTime currentDate = DateTime.UtcNow;
            Kud105 log =  new Kud105();
            log.Company = company;
            log.Key3 = $"{orderNum}~{1}~{1}~{journeyId}";
            log.Key4 = partNum;
            log.Character01 = description;
            log.Character02 = startLocation; //start Location
            log.Character03 = endLocation; //End Location
            log.Character04 = actualLocation; //Actual TruckLocation
            log.ShortChar01 = status;
            log.ShortChar02 = "";
            log.ShortChar03 = "";
            log.Key1 = "FMSLOGS";
            log.Key2 = currentDate.ToString("yyyyMMddHHmmssfff");
            log.ShortChar04 = currentDate.ToString("HH:MM:ss"); //time
            log.Key5 = Guid.NewGuid().ToString();
            log.Date01= currentDate;

            //On Epicor number1, numebr02 is integer no need benifit of saving these values
            try
            {
                if (!string.IsNullOrEmpty(log.Character02))
                {
                    var locations = log.Character02!.Split("~");
                    if (locations.Any() && locations.Length == 2)
                    {
                        log.Character02 = await HelperMethods.GetFormattedAddress(locations[0], locations[1]);
                    }
                }
                if (!string.IsNullOrEmpty(log.Character03))
                {
                    var locations = log.Character03!.Split("~");
                    if (locations.Any() && locations.Length == 2)
                    {
                        log.Character03 = await HelperMethods.GetFormattedAddress(locations[0], locations[1]);

                    }
                }
                if (!string.IsNullOrEmpty(log.Character04))
                {
                    var locations = log.Character04!.Split("~");
                    if (locations.Any() && locations.Length == 2)
                    {
                        log.ShortChar02 = locations[0];
                        log.ShortChar03 = locations[1];
                        log.Character04 = await HelperMethods.GetFormattedAddress(locations[0], locations[1]);

                    }
                }
            }
            catch (Exception ex)
            {
                await this.LogExceptionAsync(ex);
            }


            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_authorizationKey)) return ServiceResponse<string>.Return422("Missing API Keys in App Settings");
            RestClient client = new RestClient(_configuration["kairos:API_Efx_Url"] ?? string.Empty);
            var request = new RestRequest($"KUD105/CreateFunction");
            request.Timeout = -1;
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");

            var body = JsonSerializer.Serialize(log);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await client.ExecutePostAsync(request);
            if (response.IsSuccessful)
                return ServiceResponse<string>.ReturnResultWith201(response.Content!);

            return ServiceResponse<string>.ReturnFailed((int)response.StatusCode, response.Content ?? "Failed");
        }
    }
   
}
