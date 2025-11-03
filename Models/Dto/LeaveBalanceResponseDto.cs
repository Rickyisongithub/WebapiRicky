namespace KairosWebAPI.Models.Dto
{
    public class LeaveBalanceResponseDto
    {
        public string PartNum { get; set; }
        public string Description { get; set; }
        public LeaveStats Stats { get; set; }
        public List<LeaveRecordDto> Leaves { get; set; } = new();
    }
}
