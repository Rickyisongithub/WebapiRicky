namespace KairosWebAPI.Models.FilterDtos
{
    public class JourneyFilterParams : FilterParams
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? VehicleNum { get; set; }
        public string? CustomerName { get; set; }
        public decimal? OrderNum{ get; set; }

    }
}
