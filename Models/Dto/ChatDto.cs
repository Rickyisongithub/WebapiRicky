using Microsoft.AspNetCore.Mvc;

namespace KairosWebAPI.Models.Dto
{

    public class CreateChatDto
    {
        [FromForm(Name = "TruckId")]
        public string? TruckId { get; set; }
        [FromForm(Name = "Type")]
        public string? Type { get; set; }
        [FromForm(Name = "Status")]
        public string? Status { get; set; }
        [FromForm(Name = "Message")]
        public string? Message { get; set; }
        [FromForm(Name = "File")]
        public IFormFile? File { get; set; }
        [FromForm(Name = "FileType")]
        public string? FileType { get; set; }

    }
    public class UpdateChatDto
    {
        public long Id { get; set; }
        public string? TruckId { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }

    }

    public class UpdateChatStatusDto
    {
        public long? Id { get; set; }
        public string? Status { get; set; }
    }
}
