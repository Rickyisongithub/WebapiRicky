namespace KairosWebAPI.Models.Dto
{
    public class TruckStatsDto
    {
        public string? PartNum { get; set; }
        public string? Description { get; set; }
        public decimal TotalRevenue { get; set; }
        public double TotalDistance { get; set; } // in kilometers
    }
}