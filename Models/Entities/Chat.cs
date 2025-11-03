using System.ComponentModel.DataAnnotations;

namespace KairosWebAPI.Models.Entities
{
    public class Chat
    {
        [Key]
        public long Id { get; set; }
        [MaxLength(100)]
        public string? TruckId { get; set; }
        [MaxLength(100)]
        public string? Type { get; set; }
        [MaxLength(100)]
        public string? TimeStamp { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
        [MaxLength(500)]
        public string? Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        [MaxLength(500)]
        public string? FileUrl { get; set; }
        [MaxLength(30)]
        public string? FileType { get; set; }
    }
}
