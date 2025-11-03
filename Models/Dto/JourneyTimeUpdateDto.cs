namespace KairosWebAPI.Models.Dto
{
    public class JourneyTimeUpdateDto
    {
        public long Id { get; set; }
        public DateTime TravelStartTime { get; set; }
        public DateTime TravelEndTime { get; set; }
    }
}
