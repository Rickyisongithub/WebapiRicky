using System.ComponentModel.DataAnnotations;
// ReSharper disable InconsistentNaming

namespace KairosWebAPI.Models.FilterDtos
{
    public class FMSOrderParams : FilterParams
    {
        [Required]
        public DateTime OrderDate { get; set; }
    }
}
