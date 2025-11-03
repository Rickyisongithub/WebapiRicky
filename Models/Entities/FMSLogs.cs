// ReSharper disable InconsistentNaming
using System.ComponentModel.DataAnnotations;

namespace KairosWebAPI.Models.Entities
{
    public class FMSLogs
    {
        public long Id { get; set; }
        [MaxLength(10000)]
        public string? Description { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
        [MaxLength(50)]
        public string? LogType { get; set; }
        public long OrderNum { get; set; }
        public long JobNum { get; set; }
        public long PackNum { get; set; }
        public DateTime? TimeStamp { get; set; }
        [MaxLength(250)]
        public string? Location { get; set; }
        [MaxLength(10000)]
        public string? Exception { get; set; }

    }
}
