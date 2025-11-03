using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ReSharper disable InconsistentNaming

namespace KairosWebAPI.Models.Entities
{
    [Table("Trucks")]
    public class Truck
    {
        [Key]
        public long Id { get; set; }
        [MaxLength(20)]
        public string? Part_Company { get; set; }
        [MaxLength(100)]
        public string? Part_PartNum { get; set; }
        [MaxLength(100)]
        public string? Part_PartDescription { get; set; }
        public decimal? Part_UnitPrice { get; set; }
        [MaxLength(10)]
        public string? Part_SalesUM { get; set; }
        [MaxLength(250)]
        public string? CurrentLocation { get; set; }
        public float? Commission { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
    } 
}
