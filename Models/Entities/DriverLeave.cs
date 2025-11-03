using KairosWebAPI.Models.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KairosWebAPI.Models.Entities;

public class DriverLeave
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string? DriverName { get; set; }
    [MaxLength(100)]
    public string? LeaveType { get; set; }
    [MaxLength(500)]
    public string? LeaveReason { get; set; }
    public DateTime LeaveDate { get; set; }
    public int Days { get; set; }
    public ApprovalStatus Approved { get; set; } = ApprovalStatus.Pending;
    [MaxLength(100)]
    public string? ApprovedBy { get; set; }
}