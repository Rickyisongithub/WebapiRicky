using System.ComponentModel.DataAnnotations;
// ReSharper disable InconsistentNaming

namespace KairosWebAPI.Models.Dto
{
    public class JourneyCreatDto
    {
        [Required]
        public string? Company { get; set; }
        [Required]
        public string? VehicleNum { get; set; }
        [Required]
        public decimal? CustomerNum { get; set; }
        [Required]
        public string? CustomerId { get; set; }
        [Required]
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        //[Required]
        public string? StartLocation { get; set; }
        //[Required]
        public string? EndLocation { get; set; }
        [Required]
        public DateTime? JourneyDate { get; set; }
        [Required]
        public DateTime? StartTime { get; set; }
        [Required]
        public int Hours { get; set; }
        //public bool AllDay { get; set; }
        [Required]
        public bool UseOTS { get; set; }
        public string? OTSName { get; set; }
        public string? OTSAddress1 { get; set; }
        public string? OTSAddress2 { get; set; }
        public string? OTSCity { get; set; }
        public string? OTSZip { get; set; }
        public string? PONum { get; set; }
        public string? EntryPerson { get; set; }
        public decimal? OrderNum { get; set; }
        public List<JourneyDetailDto>? JourneyDetails { get; set; }
        public List<JourneyTruckDto>? JourneyTrucks { get; set; }

    }
    public class JourneyTruckDto
    {
        public long? Id { get; set; }
        public long? JourneyId { get; set; }
        public string? TruckId { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public int? Hours { get; set; }
        public IEnumerable<JourneyTruckLocationDto>? TruckLocations { get; set; }
    }
    public class JourneyTruckLocationDto
    {
        public long? Id { get; set; }
        public long JourneyTruckId { get; set; }
        public string? Location { get; set; }
        public string? Reason { get; set; }
        public int? Hours { get; set; }
    }

}
