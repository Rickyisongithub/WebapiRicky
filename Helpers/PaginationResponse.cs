namespace KairosWebAPI.Helpers
{
    public class PaginationResponse<T>
    {
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<T>? Items { get; set; }
    }

}
