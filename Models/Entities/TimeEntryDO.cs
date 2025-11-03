namespace KairosWebAPI.Models.Entities;

public class TimeEntryDO
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public DateTime StartDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? EmpId { get; set; }
    public string? LaborType { get; set; }
    public int? JobNum { get; set; }
    public string? JobDescription { get; set; }
    public string? Comments { get; set; }
    public bool Overtime { get; set; }
    public string? Status { get; set; }
    public bool Approved { get; set; }
    public bool Rejected { get; set; }
}