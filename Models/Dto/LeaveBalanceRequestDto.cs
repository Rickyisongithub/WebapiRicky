namespace KairosWebAPI.Models.Dto
{
    public class LeaveBalanceRequestDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<string> Trucks { get; set; }
    }
}
