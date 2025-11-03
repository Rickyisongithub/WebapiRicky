using System.Text.Json.Serialization;
using KairosWebAPI.Models.Dto;
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace KairosWebAPI.Models.ResponseResults
{
    public class KVendorListResponse
    {
        [JsonPropertyName("@odata.context")]
        public string? odatacontext { get; set; }
        public List<Vendor>? value { get; set; }
    }



}
