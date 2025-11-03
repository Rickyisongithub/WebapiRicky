namespace KairosWebAPI.Models.Dto
{
    public class LeaveRequestDto
    {
        public string DriverName { get; set; }
        public string LeaveType { get; set; }
        public int DaysRequested { get; set; }
    }
}
