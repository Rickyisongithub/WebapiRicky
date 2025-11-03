using System.Text.Json.Serialization;
// ReSharper disable InconsistentNaming

namespace KairosWebAPI.Models.ResponseResults.SaleOrder
{
	public class AttachmentsValue
    {
        public string? Company { get; set; }
        public string? RelatedToSchemaName { get; set; }
        public string? RelatedToFile { get; set; }
        public string? Key1 { get; set; }
        public string? Key2 { get; set; }
        public string? Key3 { get; set; }
        public string? Key4 { get; set; }
        public int AttachNum { get; set; }
        public string? Key5 { get; set; }
        public int XFileRefNum { get; set; }
        public bool DoTrigger { get; set; }
        public string? DupToRelatedToFile { get; set; }
        public string? DupToKey1 { get; set; }
        public string? DupToKey2 { get; set; }
        public string? DupToKey3 { get; set; }
        public string? DupToKey4 { get; set; }
        public string? DupToKey5 { get; set; }
        public int DupToAttachNum { get; set; }
        public string? Key6 { get; set; }
        public string? SharePointID { get; set; }
        public string? ForeignSysRowID { get; set; }
        public int SysRevID { get; set; }
        public string? SysRowID { get; set; }
        public int BitFlag { get; set; }
        public string? XFileRefDocTypeID { get; set; }
        public string? XFileRefPDMDocID { get; set; }
        public string? XFileRefXFileName { get; set; }
        public string? XFileRefXFileDesc { get; set; }
        public string? RowMod { get; set; }
    }
    public class AttachmentsResponse<T>
    {
        [JsonPropertyName("@odata.context")]
        public string? odatacontext { get; set; }
        public List<T>? value { get; set; }
    }
}


