using KairosWebAPI.Models.Entities;

namespace KairosWebAPI.Models.Dto
{
    public class VehicleDto
    {
        public string? PartNum { get; set; }
        public string? Description { get; set; }
        public List<Journey>? Journeys { get; set; }
    }


}
