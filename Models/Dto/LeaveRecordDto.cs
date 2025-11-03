namespace KairosWebAPI.Models.Dto
{
    public class LeaveRecordDto
    {
        public int Id { get; set; }
        public string DriverName { get; set; }
        public string LeaveType { get; set; }
        public string LeaveReason { get; set; }
        public string LeaveDate { get; set; }
        public bool? Approved { get; set; }
        public string ApprovedBy { get; set; }
    }
}
