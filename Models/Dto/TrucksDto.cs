// ReSharper disable InconsistentNaming
namespace KairosWebAPI.Models.Dto
{
    public class TrucksDto
    {
        public string? Part_Company { get; set; }
        public string? Part_PartNum { get; set; }
        public string? Part_PartDescription { get; set; }
        public decimal? Part_UnitPrice { get; set; }
        public string? Part_SalesUM { get; set; }
        //public string? CurrentLocation { get; set; }
        //public float? Commission { get; set; }
        //public string? Status { get; set; }
    }
}
