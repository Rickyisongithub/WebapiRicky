using KairosWebAPI.Models.Entities;

namespace KairosWebAPI.Models.Dto
{
    public class TruckDetailDto
    {
        public Truck? Truck { get; set; }
        public List<Journey>? JobHistory { get; set; }
        public JourneyInformationDto? Job { get; set; }
    }

    public class JourneyInformationDto
    {
        public Journey? Job { get; set; }
        public List<JourneyTruck>? TruckDetails { get; set; }
    }
}
