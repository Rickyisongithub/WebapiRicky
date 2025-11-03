using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KairosWebAPI.Models.Entities
{

    public class DriverLeaveBalance
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string DriverName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LeaveType { get; set; } = string.Empty;

        public int TotalLeaves { get; set; }
        public int UsedLeaves { get; set; }

        [NotMapped]
        public int RemainingLeaves => TotalLeaves - UsedLeaves;
    }
}