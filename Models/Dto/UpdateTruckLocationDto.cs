using System.ComponentModel.DataAnnotations;

namespace KairosWebAPI.Models.Dto
{
    public class UpdateTruckLocationDto
    {
        [Required]
        public string? PartNum { get; set; }
        [Required]
        public string? Location { get; set; }
    }
}
