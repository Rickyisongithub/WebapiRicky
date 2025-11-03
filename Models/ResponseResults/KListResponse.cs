using System.Text.Json.Serialization;
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace KairosWebAPI.Models.ResponseResults
{
    public class KListResponse<T>
    {
        [JsonPropertyName("@odata.context")]
        public string? odatacontext { get; set; }
        [JsonPropertyName("@odata.count")]
        public int? odatacount { get; set; }
        public List<T>? value { get; set; }
    }
}