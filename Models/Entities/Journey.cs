using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace KairosWebAPI.Models.Entities
{
    public class Journey
    {
        [Key]
        public long Id  { get; set; }
        [MaxLength(50)]
        public string? CustomerId { get; set; }
        [MaxLength(100)]
        public string? CustomerName{ get; set; }
        public decimal? CustNum { get; set; }
        [MaxLength(250)]
        public string? CustomerAddress { get; set; }
        [MaxLength(250)]
        public string? StartLocation { get; set; }
        [MaxLength(250)]
        public string? EndLocation { get; set; }
        public DateTime? TravelStartTime { get; set; }
        public DateTime? TravelEndTime { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Hours { get; set; }
        public decimal? OrderNum { get; set; }
        [MaxLength(20)]
        public string? Company { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? NeedByDate { get; set; }
        [MaxLength(100)]
        public string? PartNum { get; set; }
        [MaxLength(100)]
        public string? VehicleNum { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
        [MaxLength(50)]
        public string? PONum { get; set; }
        [MaxLength(100)]
        public string? EntryPerson { get; set; }
        public int? ActualHours { get; set; }
        [MaxLength(250)]
        public string? OrderHed_SysRowId { get; set; }
        [MaxLength(250)]
        public string? CurrentLocation { get; set; }
        public int? PackNum { get; set; }
        public List<JourneyDetail>? JourneyDetails { get; set; }
        public List<JourneyTruck>? JourneyTrucks { get; set; }
        [MaxLength(10)]
        public string? RowMod { get; set; }
    }
    //Journey Detail is used for storing Intermediate Locations of main truck
    public class JourneyDetail
    {
        [Key]
        public long Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [MaxLength(250)]
        public string? Location { get; set; }
        [MaxLength(500)]
        public string? Reason { get; set; }
        public int? Hours { get; set; }
        public long JourneyId { get; set; }

        [ForeignKey("JourneyId")]
        public Journey? Journey { get; set; }
        [MaxLength(10)]
        public string? RowMod { get; set; }
    }

    public class JourneyTruck
    {
        [Key]
        public long Id { get; set; }
        public long JourneyId { get; set; }
        [MaxLength(100)]
        public string? TruckId { get; set; }
        [MaxLength(250)]
        public string? StartLocation { get; set; }
        [MaxLength(250)]
        public string? EndLocation { get; set; }
        public int? Hours { get; set; }

        [ForeignKey("JourneyId")]
        public Journey? Journey { get; set; }
        [MaxLength(10)]
        public string? RowMod { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
        public int? ActualHours { get; set; }
        public ICollection<JourneyTruckLocation>? TruckLocations { get; set; }
    }
    public class JourneyTruckLocation
    {
        [Key]
        public long Id { get; set; }
        public long JourneyTruckId { get; set; }
        [MaxLength(250)]
        public string? Location { get; set; }
        [MaxLength(500)]
        public string? Reason { get; set; }
        public int? Hours { get; set; }
        [MaxLength(10)]
        public string? RowMod { get; set; }
        [ForeignKey("JourneyTruckId")]
        public JourneyTruck? JourneyTruck { get; set; }
    }
}
