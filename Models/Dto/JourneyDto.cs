namespace KairosWebAPI.Models.Dto
{
    public class JourneyDetailDto
    {
        public long Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public string? Reason { get; set; }
        public int? Hours { get; set; }
        public long JourneyId { get; set; }

    }
    public class JourneyDto
    {
       
        public long Id { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime? TravelStartTime { get; set; }
        public DateTime? TravelEndTime { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Hours { get; set; }
        public decimal? OrderNum { get; set; }
        public string? Company { get; set; }
        public decimal? CustNum { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? NeedByDate { get; set; }
        public string? PartNum { get; set; }
        public string? VehicleNum { get; set; }
        public List<JourneyDetailDto>? JourneyDetails { get; set; }
    }
}
