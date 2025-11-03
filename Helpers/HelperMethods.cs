using KairosWebAPI.Models.Dto;
using KairosWebAPI.Models.ResponseResults.GoogleLocation;
using RestSharp;

namespace KairosWebAPI.Helpers
{
    public class HelperMethods
    {
        public static string AddTime(string startTime, int hoursToAdd)
        {
            // Parse the input time string to get the hour, minute, and AM/PM indicator
            if (!DateTime.TryParse(startTime, out DateTime parsedTime)) return "Invalid time format.";
            // Add the specified number of hours to the parsed time
            var resultTime = parsedTime.AddHours(hoursToAdd);

            // Convert the result time to the desired format "hh:mm tt"
            var resultTimeString = resultTime.ToString("h:mm tt");

            return resultTimeString;

            // If parsing fails, return an error message or handle the exception as needed.
        }
        public static int CalculateTotalAssignedHours(VehicleDto truck)
        {
            if (truck.Journeys == null || !truck.Journeys.Any()) return 0;

            return truck.Journeys!.Sum(x => x.Hours.GetValueOrDefault());
        }
        public static async Task<string> GetLongLatInStringByAddress(string address)
        {
            var locationResponse = await HelperMethods.GetGoogleLocationDetails(address);
            var location = "no-location";
            if (locationResponse is not { Success: true, Data: not null } || locationResponse.Data!.results!.Count <= 0)
                return location;
            var loc = locationResponse.Data.results.FirstOrDefault()!.geometry!.location;
            if (loc != null)
            {
                location = $"{loc.lat}~{loc.lng}";
            }
            return location;
        }
        public static async Task<string> GetFormattedAddress(string lat,string lng)
        {
            var locationResponse = await HelperMethods.GetGoogleLocationDetailsByLatlong(lat,lng);
            var address = "Address-Not-Found";
            if (locationResponse is { Success: true, Data: not null } && locationResponse.Data!.results!.Count > 0)
            {
                address = locationResponse.Data.results.FirstOrDefault()!.formatted_address!;
            }
            return address;
        }

        public static async Task<ServiceResponse<LocationResponse>> GetGoogleLocationDetails(string address)
        {
            var client = new RestClient("https://maps.googleapis.com/maps/api/geocode/json");
            var request = new RestRequest
            {
                Timeout = -1
            };
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("address", address);
            request.AddParameter("key", "AIzaSyAID140pRxVnvbU3wU_ehmhCFobjTxfwck");

            var response = await client.ExecuteGetAsync<LocationResponse>(request);
            return response is { IsSuccessful: true, Data: not null }
                ? ServiceResponse<LocationResponse>.ReturnResultWith200(response.Data)
                : ServiceResponse<LocationResponse>.ReturnFailed((int)response.StatusCode,
                    response.ErrorMessage ?? "Failed");
        }

        private static async Task<ServiceResponse<LocationResponse>> GetGoogleLocationDetailsByLatlong(string lat,string lng)
        {
            var client = new RestClient("https://maps.googleapis.com/maps/api/geocode/json");
            var request = new RestRequest
            {
                Timeout = -1
            };
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("latlng", $"{lat},{lng}");
            request.AddParameter("key", "AIzaSyAID140pRxVnvbU3wU_ehmhCFobjTxfwck");

            var response = await client.ExecuteGetAsync<LocationResponse>(request);
            return response is { IsSuccessful: true, Data: not null }
                ? ServiceResponse<LocationResponse>.ReturnResultWith200(response.Data)
                : ServiceResponse<LocationResponse>.ReturnFailed((int)response.StatusCode,
                    response.ErrorMessage ?? "Failed");
        }
        public static double GetDistanceBetweenPoints(string origin, string destination)
        {
            try
            {
                var originCoords = origin.Split('~');
                var destCoords = destination.Split('~');

                var lat1 = double.Parse(originCoords[0]);
                var lon1 = double.Parse(originCoords[1]);
                var lat2 = double.Parse(destCoords[0]);
                var lon2 = double.Parse(destCoords[1]);

                const double R = 6371; // Radius of the Earth in kilometers

                var dLat = (lat2 - lat1) * (Math.PI / 180);
                var dLon = (lon2 - lon1) * (Math.PI / 180);

                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var distance = R * c;

                return distance; // Distance in kilometers
            }
            catch (Exception)
            {
                // Return 0 if coordinates are invalid
                return 0;
            }
        }

    }
}
