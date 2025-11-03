namespace KairosWebAPI.Models.FilterDtos
{
    public class FilterParams
    {
        public string? Select { get; set; }
        public string? Expand { get; set; }
        public string? Filter { get; set; }
        public string? OrderBy { get; set; } = string.Empty;
        public int? Top { get; set; }
        public int? Skip { get; set; } = 0;
        public bool? Count { get; set; }
    }
}
