using KairosWebAPI.Models.Dto;
using System.Text.Json.Serialization;
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace KairosWebAPI.Models.ResponseResults
{
    public class KOrdersListResponse
    {
        [JsonPropertyName("@odata.context")]
        public string? odatacontext { get; set; }
        public List<SalesOrder>? value { get; set; }
    }    

}
