using KairosWebAPI.Models.Dto;
using KairosWebAPI.Services.Jobs;
using Microsoft.AspNetCore.SignalR;

namespace KairosWebAPI.Services
{
    public class JourneyHub:Hub
    {
        private readonly IJobsService _jobsService;

        public JourneyHub(IJobsService jobsService)
        {
            _jobsService = jobsService; 
        }
        public async Task SendMessage(JourneyCreatDto model)
        {
            var response = await _jobsService.CreateJourney(model);
            if (response.Success && response.Data != null)
            {
                await Clients.All.SendAsync("JourneyMethod", model);

            }
        }
        public Task SendMessage1(string user, string message)               // Two parameters accepted
        {
            return Clients.All.SendAsync("JourneyMethodReceiveOne", user, message);    // Note this 'ReceiveOne' 
        }
    }
}
