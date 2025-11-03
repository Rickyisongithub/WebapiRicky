using System.ComponentModel.DataAnnotations;
// ReSharper disable InconsistentNaming

namespace KairosWebAPI.Models.Dto
{
	public class SaveAttachmentDto
	{
		[Required]
		public string? Base64Image { get; set; }
        public string? Company { get; set; }
        public string? OrderHed_SysRowID { get; set; }
		public string? Key1 { get; set; }
		public string? XFileRefXFileDesc { get; set; }
		public string? XFileRefDocTypeID { get; set; }
    }

    public class SaveAttachmentEpicorDTO
    {
        public string? Company { get; set; }
        public string? ForeignSysRowID { get; set; }
        public string? RelatedToSchemaName { get; set; }
        public string? RelatedToFile { get; set; }
        public string? Key1 { get; set; }
        public string? XFileRefXFileName { get; set; }
        public string? XFileRefXFileDesc { get; set; }
        public string? XFileRefDocTypeID { get; set; }
    }
}