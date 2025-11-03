namespace KairosWebAPI.Models.Dto
{
    public class LeaveStats
    {
        public int Allowed { get; set; }
        public int Applied { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
    }
}
